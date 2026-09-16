using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Talabi.Customers;
using Talabi.Interactions;
using Talabi.Interactions.Dtos;
using Talabi.Products;
using Talabi.Stores;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Xunit;

namespace Talabi.EntityFrameworkCore.Applications;

/// <summary>
/// اختبارات وحدة وتكامل خدمة إدارة المفضلة للعملاء FavoriteAppService
/// تختبر إضافة وإزالة المتاجر والمنتجات، وفحص حالة التفضيل، واسترجاع القوائم المفضلة
/// </summary>
[Collection(TalabiTestConsts.CollectionDefinitionName)]
public class FavoriteAppServiceTests : TalabiEntityFrameworkCoreTestBase
{
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public FavoriteAppServiceTests()
    {
        _guidGenerator = GetRequiredService<IGuidGenerator>();
        _currentUser = GetRequiredService<ICurrentUser>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private async Task WithUowAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var uow = uowManager.Begin(new AbpUnitOfWorkOptions());
        await action(scope.ServiceProvider);
        await uow.CompleteAsync();
    }

    private async Task<TResult> WithUowAsync<TResult>(Func<IServiceProvider, Task<TResult>> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var uow = uowManager.Begin(new AbpUnitOfWorkOptions());
        var result = await action(scope.ServiceProvider);
        await uow.CompleteAsync();
        return result;
    }

    private IDisposable ChangeUser(Guid userId, string? userName = "customer_user")
    {
        var claims = new System.Collections.Generic.List<Claim>
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, userName ?? "customer_user")
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        return _currentPrincipalAccessor.Change(principal);
    }

    private async Task<(Customer customer, Store store, Product product)> CreateTestCustomerStoreAndProductAsync()
    {
        return await WithUowAsync(async sp =>
        {
            var userRepo = sp.GetRequiredService<IRepository<IdentityUser, Guid>>();
            var customerRepo = sp.GetRequiredService<IRepository<Customer, Guid>>();
            var storeTypeRepo = sp.GetRequiredService<IRepository<StoreType, Guid>>();
            var storeRepo = sp.GetRequiredService<IRepository<Store, Guid>>();
            var productRepo = sp.GetRequiredService<IRepository<Product, Guid>>();

            // 1. عميل
            var user = new IdentityUser(_guidGenerator.Create(), "fav_user_" + Guid.NewGuid().ToString("N")[..6], "fav@talabi.com");
            await userRepo.InsertAsync(user, autoSave: true);

            var customer = new Customer(_guidGenerator.Create(), user.Id, null, "ar");
            await customerRepo.InsertAsync(customer, autoSave: true);

            // 2. متجر
            var storeType = new StoreType(_guidGenerator.Create(), "بقالة وتموينات", 1, true);
            await storeTypeRepo.InsertAsync(storeType, autoSave: true);

            var store = new Store(
                _guidGenerator.Create(),
                _guidGenerator.Create(),
                storeType.Id,
                "تموينات السعادة",
                "happiness-store-" + Guid.NewGuid().ToString("N")[..6],
                "777111222",
                "صنعاء - التحرير",
                15.35M,
                44.20M
            );
            store.LogoUrl = "https://talabi.com/logo.png";
            store.CoverImageUrl = "https://talabi.com/cover.png";
            store.Rating = 4.8M;
            store.TotalReviews = 25;
            store.MinimumOrderAmount = 1000M;
            await storeRepo.InsertAsync(store, autoSave: true);

            // 3. منتج
            var product = new Product(
                _guidGenerator.Create(),
                store.Id,
                "عسل سدر طبيعي دوعني",
                "SKU-HONEY-01",
                30000M,
                "حبة"
            );
            product.Discount = 2000M;
            product.FinalPrice = 28000M;
            product.Description = "عسل سدر أصلي ممتاز";
            await productRepo.InsertAsync(product, autoSave: true);

            return (customer, store, product);
        });
    }

    [Fact]
    public async Task ToggleAsync_Should_Add_Store_To_Favorites_When_Not_Present()
    {
        // Arrange
        var (customer, store, _) = await CreateTestCustomerStoreAndProductAsync();

        // Act
        ToggleFavoriteResultDto result = null!;
        bool isFav = false;

        using (ChangeUser(customer.UserId))
        {
            await WithUowAsync(async sp =>
            {
                var favService = sp.GetRequiredService<IFavoriteAppService>();
                result = await favService.ToggleAsync(new ToggleFavoriteInput
                {
                    EntityType = "Store",
                    EntityId = store.Id
                });

                isFav = await favService.IsFavoriteAsync("Store", store.Id);
            });
        }

        // Assert
        result.ShouldNotBeNull();
        result.IsFavorite.ShouldBeTrue();
        result.EntityType.ShouldBe("Store");
        result.EntityId.ShouldBe(store.Id);
        isFav.ShouldBeTrue();
    }

    [Fact]
    public async Task ToggleAsync_Should_Remove_Store_From_Favorites_When_Already_Present()
    {
        // Arrange
        var (customer, store, _) = await CreateTestCustomerStoreAndProductAsync();

        using (ChangeUser(customer.UserId))
        {
            await WithUowAsync(async sp =>
            {
                var favService = sp.GetRequiredService<IFavoriteAppService>();
                // First toggle: Add
                await favService.ToggleAsync(new ToggleFavoriteInput
                {
                    EntityType = "Store",
                    EntityId = store.Id
                });
            });

            // Act: Second toggle: Remove
            ToggleFavoriteResultDto removeResult = null!;
            bool isFav = true;

            await WithUowAsync(async sp =>
            {
                var favService = sp.GetRequiredService<IFavoriteAppService>();
                removeResult = await favService.ToggleAsync(new ToggleFavoriteInput
                {
                    EntityType = "Store",
                    EntityId = store.Id
                });

                isFav = await favService.IsFavoriteAsync("Store", store.Id);
            });

            // Assert
            removeResult.ShouldNotBeNull();
            removeResult.IsFavorite.ShouldBeFalse();
            isFav.ShouldBeFalse();
        }
    }

    [Fact]
    public async Task ToggleAsync_Should_Add_Product_To_Favorites()
    {
        // Arrange
        var (customer, _, product) = await CreateTestCustomerStoreAndProductAsync();

        // Act
        ToggleFavoriteResultDto result = null!;
        bool isFav = false;

        using (ChangeUser(customer.UserId))
        {
            await WithUowAsync(async sp =>
            {
                var favService = sp.GetRequiredService<IFavoriteAppService>();
                result = await favService.ToggleAsync(new ToggleFavoriteInput
                {
                    EntityType = "Product",
                    EntityId = product.Id
                });

                isFav = await favService.IsFavoriteAsync("Product", product.Id);
            });
        }

        // Assert
        result.ShouldNotBeNull();
        result.IsFavorite.ShouldBeTrue();
        result.EntityType.ShouldBe("Product");
        result.EntityId.ShouldBe(product.Id);
        isFav.ShouldBeTrue();
    }

    [Fact]
    public async Task GetMyFavoriteStoresAsync_Should_Return_Paged_Stores_With_Full_Details()
    {
        // Arrange
        var (customer, store, _) = await CreateTestCustomerStoreAndProductAsync();

        using (ChangeUser(customer.UserId))
        {
            await WithUowAsync(async sp =>
            {
                var favService = sp.GetRequiredService<IFavoriteAppService>();
                await favService.ToggleAsync(new ToggleFavoriteInput
                {
                    EntityType = "Store",
                    EntityId = store.Id
                });
            });

            // Act
            PagedResultDto<FavoriteStoreDto> result = null!;
            await WithUowAsync(async sp =>
            {
                var favService = sp.GetRequiredService<IFavoriteAppService>();
                result = await favService.GetMyFavoriteStoresAsync(new PagedAndSortedResultRequestDto());
            });

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBe(1);
            result.Items.Count.ShouldBe(1);

            var favStore = result.Items[0];
            favStore.StoreId.ShouldBe(store.Id);
            favStore.StoreName.ShouldBe("تموينات السعادة");
            favStore.StoreLogoUrl.ShouldBe("https://talabi.com/logo.png");
            favStore.Rating.ShouldBe(4.8M);
            favStore.TotalReviews.ShouldBe(25);
            favStore.MinimumOrderAmount.ShouldBe(1000M);
        }
    }

    [Fact]
    public async Task GetMyFavoriteProductsAsync_Should_Return_Paged_Products_With_Full_Details()
    {
        // Arrange
        var (customer, store, product) = await CreateTestCustomerStoreAndProductAsync();

        using (ChangeUser(customer.UserId))
        {
            await WithUowAsync(async sp =>
            {
                var favService = sp.GetRequiredService<IFavoriteAppService>();
                await favService.ToggleAsync(new ToggleFavoriteInput
                {
                    EntityType = "Product",
                    EntityId = product.Id
                });
            });

            // Act
            PagedResultDto<FavoriteProductDto> result = null!;
            await WithUowAsync(async sp =>
            {
                var favService = sp.GetRequiredService<IFavoriteAppService>();
                result = await favService.GetMyFavoriteProductsAsync(new PagedAndSortedResultRequestDto());
            });

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBe(1);
            result.Items.Count.ShouldBe(1);

            var favProd = result.Items[0];
            favProd.ProductId.ShouldBe(product.Id);
            favProd.ProductName.ShouldBe("عسل سدر طبيعي دوعني");
            favProd.StoreId.ShouldBe(store.Id);
            favProd.StoreName.ShouldBe(store.Name);
            favProd.Price.ShouldBe(30000M);
            favProd.FinalPrice.ShouldBe(28000M);
        }
    }

    [Fact]
    public async Task ToggleAsync_Should_Throw_When_Target_Entity_Does_Not_Exist()
    {
        // Arrange
        var (customer, _, _) = await CreateTestCustomerStoreAndProductAsync();
        var fakeStoreId = Guid.NewGuid();

        // Act & Assert
        using (ChangeUser(customer.UserId))
        {
            await Should.ThrowAsync<UserFriendlyException>(async () =>
            {
                await WithUowAsync(async sp =>
                {
                    var favService = sp.GetRequiredService<IFavoriteAppService>();
                    await favService.ToggleAsync(new ToggleFavoriteInput
                    {
                        EntityType = "Store",
                        EntityId = fakeStoreId
                    });
                });
            });
        }
    }
}
