using System;
using System.Threading.Tasks;
using Talabi.Interactions.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Interactions;

/// <summary>
/// واجهة خدمة إدارة قائمة المفضلة للعملاء
/// </summary>
public interface IFavoriteAppService : IApplicationService
{
    /// <summary>
    /// إضافة أو إزالة عنصر من المفضلة (تبديل الحالة Toggle)
    /// </summary>
    Task<ToggleFavoriteResultDto> ToggleAsync(ToggleFavoriteInput input);

    /// <summary>
    /// التحقق مما إذا كان العنصر المحدد مضافاً للمفضلة للعميل الحالي
    /// </summary>
    Task<bool> IsFavoriteAsync(string entityType, Guid entityId);

    /// <summary>
    /// جلب قائمة المتاجر المفضلة للعميل الحالي مع تفاصيلها
    /// </summary>
    Task<PagedResultDto<FavoriteStoreDto>> GetMyFavoriteStoresAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// جلب قائمة المنتجات المفضلة للعميل الحالي مع تفاصيلها
    /// </summary>
    Task<PagedResultDto<FavoriteProductDto>> GetMyFavoriteProductsAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// جلب قائمة عناصر المفضلة العامة للعميل الحالي
    /// </summary>
    Task<PagedResultDto<FavoriteDto>> GetMyFavoritesAsync(GetMyFavoritesInput input);
}
