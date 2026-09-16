using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Talabi.Customers;
using Talabi.Interactions.Dtos;
using Talabi.Products;
using Talabi.Stores;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Talabi.Interactions;

/// <summary>
/// خدمة التطبيق لإدارة قائمة المفضلة للعملاء (متاجر ومنتجات)
/// تتيح الإضافة والإزالة وفحص التفضيل واستعراض القوائم المفضلة بأعلى أداء
/// </summary>
[Authorize]
public class FavoriteAppService : ApplicationService, IFavoriteAppService
{
    #region Fields & Dependencies
    private readonly IRepository<Favorite, Guid> _favoriteRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Store, Guid> _storeRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<ProductImage, Guid> _productImageRepository;
    #endregion

    #region Constructors
    /// <summary>
    /// منشئ خدمة المفضلة وحقن الاعتماديات المطلوبة
    /// </summary>
    public FavoriteAppService(
        IRepository<Favorite, Guid> favoriteRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Store, Guid> storeRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<ProductImage, Guid> productImageRepository)
    {
        _favoriteRepository = favoriteRepository;
        _customerRepository = customerRepository;
        _storeRepository = storeRepository;
        _productRepository = productRepository;
        _productImageRepository = productImageRepository;
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// إضافة أو إزالة عنصر من المفضلة (تبديل الحالة Toggle)
    /// </summary>
    public async Task<ToggleFavoriteResultDto> ToggleAsync(ToggleFavoriteInput input)
    {
        var entityType = NormalizeEntityType(input.EntityType);
        var customer = await GetCurrentCustomerAsync();

        // 1. التحقق من وجود الكيان المستهدف في قاعدة البيانات
        if (entityType.Equals("Store", StringComparison.OrdinalIgnoreCase))
        {
            var store = await _storeRepository.FindAsync(input.EntityId);
            if (store == null)
            {
                throw new UserFriendlyException("المتجر المحدد غير موجود في النظام.");
            }
        }
        else if (entityType.Equals("Product", StringComparison.OrdinalIgnoreCase))
        {
            var product = await _productRepository.FindAsync(input.EntityId);
            if (product == null)
            {
                throw new UserFriendlyException("المنتج المحدد غير موجود في النظام.");
            }
        }
        else
        {
            throw new UserFriendlyException("نوع الكيان غير مدعوم، يجب أن يكون Store أو Product.");
        }

        // 2. فحص هل العنصر مضاف مسبقاً لمفضلة العميل
        var existingFavorite = await _favoriteRepository.FirstOrDefaultAsync(
            x => x.CustomerId == customer.Id &&
                 x.EntityType == entityType &&
                 x.EntityId == input.EntityId
        );

        if (existingFavorite != null)
        {
            // إزالة من المفضلة
            await _favoriteRepository.DeleteAsync(existingFavorite, autoSave: true);

            Logger.LogInformation(
                "تمت إزالة العنصر {EntityId} ({EntityType}) من مفضلة العميل: {CustomerId}",
                input.EntityId, entityType, customer.Id);

            return new ToggleFavoriteResultDto
            {
                IsFavorite = false,
                EntityType = entityType,
                EntityId = input.EntityId,
                Message = "تمت إزالة العنصر من قائمة المفضلة بنجاح."
            };
        }
        else
        {
            // إضافة إلى المفضلة
            var favorite = new Favorite(
                GuidGenerator.Create(),
                customer.Id,
                entityType,
                input.EntityId
            );

            await _favoriteRepository.InsertAsync(favorite, autoSave: true);

            Logger.LogInformation(
                "تمت إضافة العنصر {EntityId} ({EntityType}) إلى مفضلة العميل: {CustomerId}",
                input.EntityId, entityType, customer.Id);

            return new ToggleFavoriteResultDto
            {
                IsFavorite = true,
                EntityType = entityType,
                EntityId = input.EntityId,
                Message = "تمت إضافة العنصر إلى قائمة المفضلة بنجاح."
            };
        }
    }

    /// <summary>
    /// التحقق مما إذا كان العنصر المحدد مضافاً لمفضلة العميل الحالي
    /// </summary>
    [AllowAnonymous]
    public async Task<bool> IsFavoriteAsync(string entityType, Guid entityId)
    {
        if (!CurrentUser.IsAuthenticated)
        {
            return false;
        }

        var customer = await GetCurrentCustomerOrNullAsync();
        if (customer == null)
        {
            return false;
        }

        var normalizedType = NormalizeEntityType(entityType);

        var query = await _favoriteRepository.GetQueryableAsync();
        return await AsyncExecuter.AnyAsync(
            query.Where(x => x.CustomerId == customer.Id &&
                             x.EntityType == normalizedType &&
                             x.EntityId == entityId)
        );
    }

    /// <summary>
    /// جلب قائمة المتاجر المفضلة للعميل الحالي مع تفاصيلها
    /// </summary>
    public async Task<PagedResultDto<FavoriteStoreDto>> GetMyFavoriteStoresAsync(PagedAndSortedResultRequestDto input)
    {
        var customer = await GetCurrentCustomerAsync();

        var query = (await _favoriteRepository.GetQueryableAsync())
            .Where(x => x.CustomerId == customer.Id && x.EntityType == "Store");

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? $"{nameof(Favorite.CreationTime)} desc"
            : input.Sorting;

        var pagedFavorites = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).PageBy(input)
        );

        if (pagedFavorites.Count == 0)
        {
            return new PagedResultDto<FavoriteStoreDto>(totalCount, new List<FavoriteStoreDto>());
        }

        var storeIds = pagedFavorites.Select(f => f.EntityId).Distinct().ToList();
        var stores = (await _storeRepository.GetListAsync(s => storeIds.Contains(s.Id)))
            .ToDictionary(s => s.Id);

        var resultList = new List<FavoriteStoreDto>();
        foreach (var fav in pagedFavorites)
        {
            if (stores.TryGetValue(fav.EntityId, out var store))
            {
                resultList.Add(new FavoriteStoreDto
                {
                    FavoriteId = fav.Id,
                    StoreId = store.Id,
                    StoreName = store.Name,
                    StoreLogoUrl = store.LogoUrl,
                    StoreCoverImageUrl = store.CoverImageUrl,
                    Address = store.Address,
                    Rating = store.Rating,
                    TotalReviews = store.TotalReviews,
                    MinimumOrderAmount = store.MinimumOrderAmount,
                    AddedAt = fav.CreationTime
                });
            }
        }

        return new PagedResultDto<FavoriteStoreDto>(totalCount, resultList);
    }

    /// <summary>
    /// جلب قائمة المنتجات المفضلة للعميل الحالي مع تفاصيلها
    /// </summary>
    public async Task<PagedResultDto<FavoriteProductDto>> GetMyFavoriteProductsAsync(PagedAndSortedResultRequestDto input)
    {
        var customer = await GetCurrentCustomerAsync();

        var query = (await _favoriteRepository.GetQueryableAsync())
            .Where(x => x.CustomerId == customer.Id && x.EntityType == "Product");

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? $"{nameof(Favorite.CreationTime)} desc"
            : input.Sorting;

        var pagedFavorites = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).PageBy(input)
        );

        if (pagedFavorites.Count == 0)
        {
            return new PagedResultDto<FavoriteProductDto>(totalCount, new List<FavoriteProductDto>());
        }

        var productIds = pagedFavorites.Select(f => f.EntityId).Distinct().ToList();
        var products = (await _productRepository.GetListAsync(p => productIds.Contains(p.Id)))
            .ToDictionary(p => p.Id);

        var storeIds = products.Values.Select(p => p.StoreId).Distinct().ToList();
        var stores = (await _storeRepository.GetListAsync(s => storeIds.Contains(s.Id)))
            .ToDictionary(s => s.Id, s => s.Name);

        // جلب الصور الأساسية للمنتجات
        var images = (await _productImageRepository.GetListAsync(img => productIds.Contains(img.ProductId)))
            .GroupBy(img => img.ProductId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(img => img.IsPrimary).FirstOrDefault()?.ImageUrl);

        var resultList = new List<FavoriteProductDto>();
        foreach (var fav in pagedFavorites)
        {
            if (products.TryGetValue(fav.EntityId, out var product))
            {
                stores.TryGetValue(product.StoreId, out var storeName);
                images.TryGetValue(product.Id, out var imageUrl);

                resultList.Add(new FavoriteProductDto
                {
                    FavoriteId = fav.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImageUrl = imageUrl,
                    StoreId = product.StoreId,
                    StoreName = storeName ?? string.Empty,
                    Price = product.Price,
                    Discount = product.Discount,
                    FinalPrice = product.FinalPrice,
                    IsActive = product.IsActive,
                    AddedAt = fav.CreationTime
                });
            }
        }

        return new PagedResultDto<FavoriteProductDto>(totalCount, resultList);
    }

    /// <summary>
    /// جلب قائمة عناصر المفضلة العامة للعميل الحالي
    /// </summary>
    public async Task<PagedResultDto<FavoriteDto>> GetMyFavoritesAsync(GetMyFavoritesInput input)
    {
        var customer = await GetCurrentCustomerAsync();

        var query = (await _favoriteRepository.GetQueryableAsync())
            .Where(x => x.CustomerId == customer.Id)
            .WhereIf(!string.IsNullOrWhiteSpace(input.EntityType), x => x.EntityType == input.EntityType);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? $"{nameof(Favorite.CreationTime)} desc"
            : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).PageBy(input)
        );

        var storeIds = items.Where(x => x.EntityType == "Store").Select(x => x.EntityId).Distinct().ToList();
        var productIds = items.Where(x => x.EntityType == "Product").Select(x => x.EntityId).Distinct().ToList();

        var stores = storeIds.Count > 0
            ? (await _storeRepository.GetListAsync(s => storeIds.Contains(s.Id))).ToDictionary(s => s.Id)
            : new Dictionary<Guid, Store>();

        var products = productIds.Count > 0
            ? (await _productRepository.GetListAsync(p => productIds.Contains(p.Id))).ToDictionary(p => p.Id)
            : new Dictionary<Guid, Product>();

        var productImages = productIds.Count > 0
            ? (await _productImageRepository.GetListAsync(img => productIds.Contains(img.ProductId)))
                .GroupBy(img => img.ProductId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(img => img.IsPrimary).FirstOrDefault()?.ImageUrl)
            : new Dictionary<Guid, string?>();

        var dtoList = new List<FavoriteDto>();
        foreach (var item in items)
        {
            var dto = new FavoriteDto
            {
                Id = item.Id,
                CustomerId = item.CustomerId,
                EntityType = item.EntityType,
                EntityId = item.EntityId,
                CreationTime = item.CreationTime,
                CreatorId = item.CreatorId
            };

            if (item.EntityType == "Store" && stores.TryGetValue(item.EntityId, out var store))
            {
                dto.EntityName = store.Name;
                dto.EntityImageUrl = store.LogoUrl;
            }
            else if (item.EntityType == "Product" && products.TryGetValue(item.EntityId, out var product))
            {
                dto.EntityName = product.Name;
                productImages.TryGetValue(product.Id, out var imgUrl);
                dto.EntityImageUrl = imgUrl;
            }

            dtoList.Add(dto);
        }

        return new PagedResultDto<FavoriteDto>(totalCount, dtoList);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// توحيد صيغة نوع الكيان (Store / Product)
    /// </summary>
    private static string NormalizeEntityType(string entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            return string.Empty;
        }

        var trimmed = entityType.Trim();
        if (trimmed.Equals("store", StringComparison.OrdinalIgnoreCase))
        {
            return "Store";
        }

        if (trimmed.Equals("product", StringComparison.OrdinalIgnoreCase))
        {
            return "Product";
        }

        return trimmed;
    }

    /// <summary>
    /// جلب كيان العميل المرتبط بالمستخدم الحالي مع رمي استثناء في حال عدم وجوده
    /// </summary>
    private async Task<Customer> GetCurrentCustomerAsync()
    {
        var currentUserId = CurrentUser.GetId();
        var customer = await _customerRepository.FirstOrDefaultAsync(c => c.UserId == currentUserId);

        if (customer == null)
        {
            throw new UserFriendlyException("لم يتم العثور على حساب عميل مرتبط بهذا المستخدم.");
        }

        return customer;
    }

    /// <summary>
    /// جلب كيان العميل المرتبط بالمستخدم الحالي إن وجد أو إرجاع null
    /// </summary>
    private async Task<Customer?> GetCurrentCustomerOrNullAsync()
    {
        if (!CurrentUser.IsAuthenticated)
        {
            return null;
        }

        var currentUserId = CurrentUser.GetId();
        return await _customerRepository.FirstOrDefaultAsync(c => c.UserId == currentUserId);
    }

    #endregion
}
