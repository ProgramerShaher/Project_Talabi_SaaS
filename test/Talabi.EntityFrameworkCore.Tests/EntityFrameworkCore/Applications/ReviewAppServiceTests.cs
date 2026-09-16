using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Talabi.Customers;
using Talabi.Interactions;
using Talabi.Interactions.Dtos;
using Talabi.Stores;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Xunit;

namespace Talabi.EntityFrameworkCore.Applications;

/// <summary>
/// اختبارات وحدة وتكامل خدمة تقييمات المتاجر ReviewAppService
/// تختبر أمان الملكية، وحساب التقييمات، وتحديث إحصائيات المتجر تلقائياً
/// </summary>
[Collection(TalabiTestConsts.CollectionDefinitionName)]
public class ReviewAppServiceTests : TalabiEntityFrameworkCoreTestBase
{
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public ReviewAppServiceTests()
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

    private IDisposable ChangeUser(Guid userId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, "testuser_" + userId.ToString("N")[..6]),
            new Claim(AbpClaimTypes.Email, "user@test.com")
        }));
        return _currentPrincipalAccessor.Change(principal);
    }

    private async Task<(Store store, Customer customer)> CreateTestStoreAndCustomerAsync()
    {
        return await WithUowAsync(async sp =>
        {
            var storeTypeRepository = sp.GetRequiredService<IRepository<StoreType, Guid>>();
            var storeRepository = sp.GetRequiredService<IRepository<Store, Guid>>();
            var customerRepository = sp.GetRequiredService<IRepository<Customer, Guid>>();

            var storeType = new StoreType(_guidGenerator.Create(), "مطاعم وتغذية", 1, true);
            await storeTypeRepository.InsertAsync(storeType, autoSave: true);

            var currentUserId = _currentUser.Id ?? _guidGenerator.Create();
            var store = new Store(
                _guidGenerator.Create(),
                currentUserId,
                storeType.Id,
                "مطعم الشام الأصيل",
                "alsham-restaurant-" + Guid.NewGuid().ToString("N")[..8],
                "777123456",
                "صنعاء - شارع حدة",
                15.35M,
                44.20M
            );
            store.Rating = 0;
            store.TotalReviews = 0;
            await storeRepository.InsertAsync(store, autoSave: true);

            var customer = await customerRepository.FirstOrDefaultAsync(c => c.UserId == currentUserId);
            if (customer == null)
            {
                customer = new Customer(_guidGenerator.Create(), currentUserId, null, "ar");
                await customerRepository.InsertAsync(customer, autoSave: true);
            }

            return (store, customer);
        });
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Review_And_Update_Store_Rating()
    {
        // Arrange
        var (store, customer) = await CreateTestStoreAndCustomerAsync();

        var input = new CreateReviewInput
        {
            StoreId = store.Id,
            StoreRating = 4,
            ProductQualityRating = 5,
            Comment = "تجربة ممتازة وجودة طعام عالية جداً"
        };

        // Act
        ReviewDto result = null!;
        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            result = await appService.CreateAsync(input);
        });

        // Assert
        result.ShouldNotBeNull();
        result.StoreId.ShouldBe(store.Id);
        result.CustomerId.ShouldBe(customer.Id);
        result.StoreRating.ShouldBe(4);
        result.ProductQualityRating.ShouldBe(5);
        result.Comment.ShouldBe("تجربة ممتازة وجودة طعام عالية جداً");

        // التحقق من تحديث إحصائيات المتجر تلقائياً
        await WithUowAsync(async sp =>
        {
            var storeRepo = sp.GetRequiredService<IRepository<Store, Guid>>();
            var updatedStore = await storeRepo.GetAsync(store.Id);
            updatedStore.Rating.ShouldBe(4.0M);
            updatedStore.TotalReviews.ShouldBe(1);
        });
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Own_Review_And_Recalculate_Store_Rating()
    {
        // Arrange
        var (store, customer) = await CreateTestStoreAndCustomerAsync();

        var createInput = new CreateReviewInput
        {
            StoreId = store.Id,
            StoreRating = 3,
            Comment = "جيد نوعاً ما"
        };

        ReviewDto created = null!;
        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            created = await appService.CreateAsync(createInput);
        });

        var updateInput = new UpdateReviewInput
        {
            StoreRating = 5,
            ProductQualityRating = 5,
            Comment = "تم تحسين الخدمة وأصبحت ممتازة 5 نجوم"
        };

        // Act
        ReviewDto updated = null!;
        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            updated = await appService.UpdateAsync(created.Id, updateInput);
        });

        // Assert
        updated.StoreRating.ShouldBe(5);
        updated.Comment.ShouldBe("تم تحسين الخدمة وأصبحت ممتازة 5 نجوم");

        await WithUowAsync(async sp =>
        {
            var storeRepo = sp.GetRequiredService<IRepository<Store, Guid>>();
            var updatedStore = await storeRepo.GetAsync(store.Id);
            updatedStore.Rating.ShouldBe(5.0M);
            updatedStore.TotalReviews.ShouldBe(1);
        });
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_When_Customer_Tries_To_Update_Others_Review()
    {
        // Arrange
        var (store, customerA) = await CreateTestStoreAndCustomerAsync();

        var createInput = new CreateReviewInput
        {
            StoreId = store.Id,
            StoreRating = 4,
            Comment = "تقييم العميل أ"
        };

        ReviewDto created = null!;
        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            created = await appService.CreateAsync(createInput);
        });

        // إنشاء عميل ثان بـ UserId مختلف
        var customerBUserId = _guidGenerator.Create();
        await WithUowAsync(async sp =>
        {
            var customerRepo = sp.GetRequiredService<IRepository<Customer, Guid>>();
            var customerB = new Customer(_guidGenerator.Create(), customerBUserId, null, "ar");
            await customerRepo.InsertAsync(customerB, autoSave: true);
        });

        // Act & Assert: تبديل المستخدم الحالي إلى العميل ب ومحاولة تعديل تقييم العميل أ
        using (ChangeUser(customerBUserId))
        {
            await WithUowAsync(async sp =>
            {
                var appService = sp.GetRequiredService<IReviewAppService>();
                var updateInput = new UpdateReviewInput
                {
                    StoreRating = 1,
                    Comment = "محاولة تعديل غير مصرح بها"
                };

                var ex = await Should.ThrowAsync<UserFriendlyException>(async () =>
                {
                    await appService.UpdateAsync(created.Id, updateInput);
                });

                ex.Message.ShouldContain("غير مصرح لك بتعديل هذا التقييم");
            });
        }
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_When_Customer_Tries_To_Delete_Others_Review()
    {
        // Arrange
        var (store, customerA) = await CreateTestStoreAndCustomerAsync();

        var createInput = new CreateReviewInput
        {
            StoreId = store.Id,
            StoreRating = 4,
            Comment = "تقييم العميل أ"
        };

        ReviewDto created = null!;
        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            created = await appService.CreateAsync(createInput);
        });

        // إنشاء عميل ثان بـ UserId مختلف
        var customerBUserId = _guidGenerator.Create();
        await WithUowAsync(async sp =>
        {
            var customerRepo = sp.GetRequiredService<IRepository<Customer, Guid>>();
            var customerB = new Customer(_guidGenerator.Create(), customerBUserId, null, "ar");
            await customerRepo.InsertAsync(customerB, autoSave: true);
        });

        // Act & Assert: تبديل المستخدم الحالي إلى العميل ب ومحاولة حذف تقييم العميل أ
        using (ChangeUser(customerBUserId))
        {
            await WithUowAsync(async sp =>
            {
                var appService = sp.GetRequiredService<IReviewAppService>();
                var ex = await Should.ThrowAsync<UserFriendlyException>(async () =>
                {
                    await appService.DeleteAsync(created.Id);
                });

                ex.Message.ShouldContain("غير مصرح لك بحذف هذا التقييم");
            });
        }
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Customer_Reviews_Same_Store_Twice()
    {
        // Arrange
        var (store, customer) = await CreateTestStoreAndCustomerAsync();

        var input = new CreateReviewInput
        {
            StoreId = store.Id,
            StoreRating = 4,
            Comment = "التقييم الأول"
        };

        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            await appService.CreateAsync(input);
        });

        // Act & Assert: محاولة التقييم مرة ثانية لنفس المتجر
        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            var ex = await Should.ThrowAsync<UserFriendlyException>(async () =>
            {
                await appService.CreateAsync(input);
            });

            ex.Message.ShouldContain("لقد قمت بتقييم هذا المتجر مسبقاً");
        });
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Own_Review_And_Recalculate_Store_Rating()
    {
        // Arrange
        var (store, customerA) = await CreateTestStoreAndCustomerAsync();

        ReviewDto reviewA = null!;
        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            reviewA = await appService.CreateAsync(new CreateReviewInput
            {
                StoreId = store.Id,
                StoreRating = 5,
                Comment = "تقييم ممتاز"
            });
        });

        // عميل ثان يقيّم بنجمة واحدة
        var customerBUserId = _guidGenerator.Create();
        await WithUowAsync(async sp =>
        {
            var customerRepo = sp.GetRequiredService<IRepository<Customer, Guid>>();
            var customerB = new Customer(_guidGenerator.Create(), customerBUserId, null, "ar");
            await customerRepo.InsertAsync(customerB, autoSave: true);
        });

        using (ChangeUser(customerBUserId))
        {
            await WithUowAsync(async sp =>
            {
                var appService = sp.GetRequiredService<IReviewAppService>();
                await appService.CreateAsync(new CreateReviewInput
                {
                    StoreId = store.Id,
                    StoreRating = 1,
                    Comment = "تقييم منخفض"
                });
            });
        }

        // المتوسط الحالي = (5 + 1) / 2 = 3.0
        await WithUowAsync(async sp =>
        {
            var storeRepo = sp.GetRequiredService<IRepository<Store, Guid>>();
            var storeAfterTwo = await storeRepo.GetAsync(store.Id);
            storeAfterTwo.Rating.ShouldBe(3.0M);
            storeAfterTwo.TotalReviews.ShouldBe(2);
        });

        // Act: العميل أ يحذف تقييمه الخاص
        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            await appService.DeleteAsync(reviewA.Id);
        });

        // Assert: التقييم المتبقي فقط هو تقييم العميل ب (1 نجمة)، والمتوسط = 1.0
        await WithUowAsync(async sp =>
        {
            var storeRepo = sp.GetRequiredService<IRepository<Store, Guid>>();
            var reviewRepo = sp.GetRequiredService<IRepository<Review, Guid>>();

            var storeAfterDelete = await storeRepo.GetAsync(store.Id);
            storeAfterDelete.Rating.ShouldBe(1.0M);
            storeAfterDelete.TotalReviews.ShouldBe(1);

            var reviewExists = await reviewRepo.FindAsync(reviewA.Id);
            reviewExists.ShouldBeNull();
        });
    }

    [Fact]
    public async Task GetStoreReviewsAsync_Should_Return_Paged_Reviews()
    {
        // Arrange
        var (store, customer) = await CreateTestStoreAndCustomerAsync();

        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            await appService.CreateAsync(new CreateReviewInput
            {
                StoreId = store.Id,
                StoreRating = 5,
                Comment = "تقييم عام"
            });
        });

        // Act
        PagedResultDto<ReviewDto> result = null!;
        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            result = await appService.GetStoreReviewsAsync(store.Id, new GetReviewListInput
            {
                MaxResultCount = 10,
                SkipCount = 0
            });
        });

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBe(1);
        result.Items.Count.ShouldBe(1);
        result.Items[0].StoreId.ShouldBe(store.Id);
        result.Items[0].StoreName.ShouldBe(store.Name);
    }

    [Fact]
    public async Task GetStoreReviewSummaryAsync_Should_Return_Correct_Distribution()
    {
        // Arrange
        var (store, customerA) = await CreateTestStoreAndCustomerAsync();

        // إنشاء تقييم 5 نجوم من العميل أ
        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            await appService.CreateAsync(new CreateReviewInput
            {
                StoreId = store.Id,
                StoreRating = 5
            });
        });

        // إنشاء تقييم 3 نجوم من العميل ب
        var customerBUserId = _guidGenerator.Create();
        await WithUowAsync(async sp =>
        {
            var customerRepo = sp.GetRequiredService<IRepository<Customer, Guid>>();
            var customerB = new Customer(_guidGenerator.Create(), customerBUserId, null, "ar");
            await customerRepo.InsertAsync(customerB, autoSave: true);
        });

        using (ChangeUser(customerBUserId))
        {
            await WithUowAsync(async sp =>
            {
                var appService = sp.GetRequiredService<IReviewAppService>();
                await appService.CreateAsync(new CreateReviewInput
                {
                    StoreId = store.Id,
                    StoreRating = 3
                });
            });
        }

        // Act
        StoreReviewSummaryDto summary = null!;
        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            summary = await appService.GetStoreReviewSummaryAsync(store.Id);
        });

        // Assert: (5 + 3) / 2 = 4.0
        summary.ShouldNotBeNull();
        summary.StoreId.ShouldBe(store.Id);
        summary.StoreName.ShouldBe(store.Name);
        summary.TotalReviews.ShouldBe(2);
        summary.AverageRating.ShouldBe(4.0M);
        summary.FiveStarCount.ShouldBe(1);
        summary.FourStarCount.ShouldBe(0);
        summary.ThreeStarCount.ShouldBe(1);
        summary.TwoStarCount.ShouldBe(0);
        summary.OneStarCount.ShouldBe(0);
    }

    [Fact]
    public async Task ReplyAsync_Should_Allow_Store_Owner_To_Reply()
    {
        // Arrange
        var (store, customer) = await CreateTestStoreAndCustomerAsync();

        ReviewDto review = null!;
        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            review = await appService.CreateAsync(new CreateReviewInput
            {
                StoreId = store.Id,
                StoreRating = 5,
                Comment = "شكراً على الخدمة"
            });
        });

        var replyInput = new StoreReplyInput
        {
            Reply = "أهلاً بك دائماً ويسعدنا خدمتك!"
        };

        // Act: مالك المتجر هو المستخدم الحالي
        ReviewDto replied = null!;
        await WithUowAsync(async sp =>
        {
            var appService = sp.GetRequiredService<IReviewAppService>();
            replied = await appService.ReplyAsync(review.Id, replyInput);
        });

        // Assert
        replied.ShouldNotBeNull();
        replied.StoreReply.ShouldBe("أهلاً بك دائماً ويسعدنا خدمتك!");
        replied.StoreRepliedAt.ShouldNotBeNull();
    }
}
