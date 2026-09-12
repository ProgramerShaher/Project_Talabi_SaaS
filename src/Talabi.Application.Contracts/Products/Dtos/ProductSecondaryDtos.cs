using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Products.Dtos;

/// <summary>
/// كائن طلب وفلترة قائمة المنتجات
/// </summary>
public class GetProductListInput : PagedAndSortedResultRequestDto
{
    #region Properties
    public string? Filter { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? StoreCategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsAvailable { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
    // public string? Brand { get; set; }
    #endregion
}

/// <summary>
/// كائن صورة المنتج
/// </summary>
public class ProductImageDto : CreationAuditedEntityDto<Guid>
{
    #region Properties
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
    #endregion
}

/// <summary>
/// كائن إضافة صورة لمنتج
/// </summary>
public class CreateProductImageDto
{
    #region Properties
    [Required(ErrorMessage = "رابط الصورة مطلوب")]
    [StringLength(ProductImageConsts.MaxImageUrlLength)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(ProductImageConsts.MaxThumbnailUrlLength)]
    public string? ThumbnailUrl { get; set; }

    [StringLength(ProductImageConsts.MaxAltTextLength)]
    public string? AltText { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
    #endregion
}

// /// <summary>
// /// كائن عرض بيانات المخزون - معطل لأن النظام للعرض والطلب فقط ولا يتدخل في كميات المخزون
// /// </summary>
// public class InventoryDto : FullAuditedEntityDto<Guid>
// {
//     #region Properties
//     public Guid? TenantId { get; set; }
//     public Guid StoreId { get; set; }
//     public Guid ProductId { get; set; }
//     public string ProductName { get; set; } = string.Empty;
//     public int CurrentQuantity { get; set; }
//     public int ReservedQuantity { get; set; }
//     public int AvailableQuantity { get; set; }
//     public int MinStockLevel { get; set; }
//     public int MaxStockLevel { get; set; }
//     public int ReorderLevel { get; set; }
//     public string? WarehouseLocation { get; set; }
//     public DateTime? LastRestockedAt { get; set; }
//     public bool IsTracked { get; set; }
//     #endregion
// }

// /// <summary>
// /// كائن تعديل وضبط كميات المخزون - معطل
// /// </summary>
// public class UpdateInventoryDto
// {
//     #region Properties
//     [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من أو تساوي صفر")]
//     public int CurrentQuantity { get; set; }
// 
//     public int MinStockLevel { get; set; }
//     public int MaxStockLevel { get; set; }
//     public int ReorderLevel { get; set; }
// 
//     [StringLength(InventoryConsts.MaxWarehouseLocationLength)]
//     public string? WarehouseLocation { get; set; }
// 
//     public bool IsTracked { get; set; }
//     #endregion
// }
