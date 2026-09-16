using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Products.Dtos;

/// <summary>
/// كائن عرض بيانات المنتج
/// </summary>
public class ProductDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public Guid? TenantId { get; set; }
    public Guid StoreId { get; set; }
    public Guid? StoreCategoryId { get; set; }
    public string StoreCategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string SKU { get; set; } = string.Empty;
    // public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal FinalPrice { get; set; }
    // public decimal? CostPrice { get; set; }
    public string Unit { get; set; } = string.Empty;
    // public string? Brand { get; set; }
    // public decimal? Weight { get; set; }
    // public string? WeightUnit { get; set; }
    // public string? Origin { get; set; }
    public string? MainImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string? Tags { get; set; }
    public int MinOrderQuantity { get; set; }
    public int? MaxOrderQuantity { get; set; }
    // public int AvailableStock { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
    public List<ProductSalesUnitDto> SalesUnits { get; set; } = new();
    #endregion
}

/// <summary>
/// كائن إضافة منتج جديد
/// </summary>
public class CreateProductDto
{
    #region Properties
    [Required(ErrorMessage = "معرف المتجر مطلوب")]
    public Guid StoreId { get; set; }

    public Guid? StoreCategoryId { get; set; }

    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [StringLength(ProductConsts.MaxNameLength, ErrorMessage = "تجاوزت الحد الأقصى لاسم المنتج")]
    public string Name { get; set; } = string.Empty;

    [StringLength(ProductConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [StringLength(ProductConsts.MaxShortDescriptionLength)]
    public string? ShortDescription { get; set; }

    [Required(ErrorMessage = "رمز المنتج SKU مطلوب")]
    [StringLength(ProductConsts.MaxSkuLength)]
    public string SKU { get; set; } = string.Empty;

    // [StringLength(ProductConsts.MaxBarcodeLength)]
    // public string? Barcode { get; set; }

    [Required(ErrorMessage = "سعر المنتج مطلوب")]
    [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون السعر أكبر من صفر")]
    public decimal Price { get; set; }

    public decimal Discount { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.Fixed;
    // public decimal? CostPrice { get; set; }

    [Required(ErrorMessage = "وحدة القياس مطلوبة")]
    [StringLength(ProductConsts.MaxUnitLength)]
    public string Unit { get; set; } = string.Empty;

    // [StringLength(ProductConsts.MaxBrandLength)]
    // public string? Brand { get; set; }

    // public decimal? Weight { get; set; }

    // [StringLength(ProductConsts.MaxWeightUnitLength)]
    // public string? WeightUnit { get; set; }

    // [StringLength(ProductConsts.MaxOriginLength)]
    // public string? Origin { get; set; }

    [StringLength(ProductConsts.MaxMainImageUrlLength)]
    public string? MainImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;

    [StringLength(ProductConsts.MaxTagsLength)]
    public string? Tags { get; set; }

    public int MinOrderQuantity { get; set; } = 1;
    public int? MaxOrderQuantity { get; set; }
    // public int InitialQuantity { get; set; } = 0;

    /// <summary>
    /// وحدات بيع المنتج الإضافية المخصصة (اختيارية بالكامل)
    /// </summary>
    public List<CreateProductSalesUnitDto>? SalesUnits { get; set; }
    #endregion
}

/// <summary>
/// كائن تعديل بيانات المنتج
/// </summary>
public class UpdateProductDto
{
    #region Properties
    public Guid? StoreCategoryId { get; set; }

    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [StringLength(ProductConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(ProductConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [StringLength(ProductConsts.MaxShortDescriptionLength)]
    public string? ShortDescription { get; set; }

    [Required(ErrorMessage = "رمز المنتج SKU مطلوب")]
    [StringLength(ProductConsts.MaxSkuLength)]
    public string SKU { get; set; } = string.Empty;

    // [StringLength(ProductConsts.MaxBarcodeLength)]
    // public string? Barcode { get; set; }

    [Required(ErrorMessage = "سعر المنتج مطلوب")]
    [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون السعر أكبر من صفر")]
    public decimal Price { get; set; }

    public decimal Discount { get; set; }
    public DiscountType DiscountType { get; set; }
    // public decimal? CostPrice { get; set; }

    [Required(ErrorMessage = "وحدة القياس مطلوبة")]
    [StringLength(ProductConsts.MaxUnitLength)]
    public string Unit { get; set; } = string.Empty;

    // [StringLength(ProductConsts.MaxBrandLength)]
    // public string? Brand { get; set; }

    // public decimal? Weight { get; set; }

    // [StringLength(ProductConsts.MaxWeightUnitLength)]
    // public string? WeightUnit { get; set; }

    // [StringLength(ProductConsts.MaxOriginLength)]
    // public string? Origin { get; set; }

    [StringLength(ProductConsts.MaxMainImageUrlLength)]
    public string? MainImageUrl { get; set; }

    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }

    [StringLength(ProductConsts.MaxTagsLength)]
    public string? Tags { get; set; }

    public int MinOrderQuantity { get; set; }
    public int? MaxOrderQuantity { get; set; }

    /// <summary>
    /// وحدات بيع المنتج الإضافية المخصصة (اختيارية بالكامل)
    /// </summary>
    public List<UpdateProductSalesUnitDto>? SalesUnits { get; set; }
    #endregion
}
