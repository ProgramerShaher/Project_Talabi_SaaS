using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Products.Dtos;

/// <summary>
/// كائن عرض وحدة البيع المربوطة بالمنتج
/// </summary>
public class ProductSalesUnitDto : FullAuditedEntityDto<Guid>
{
    #region Properties

    /// <summary>
    /// معرف المنتج
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// معرف وحدة البيع الأصلية
    /// </summary>
    public Guid SalesUnitId { get; set; }

    /// <summary>
    /// اسم وحدة البيع (مثال: كيلو، كيس)
    /// </summary>
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// سعر بيع المنتج بهذه الوحدة
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// هل هي الوحدة الافتراضية للمنتج؟
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// هل الوحدة متاحة للشراء؟
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// ترتيب الظهور
    /// </summary>
    public int DisplayOrder { get; set; }

    #endregion
}

/// <summary>
/// كائن إضافة أو تخصيص وحدة بيع للمنتج (اختياري)
/// </summary>
public class CreateProductSalesUnitDto
{
    #region Properties

    /// <summary>
    /// معرف وحدة البيع الأصلية
    /// </summary>
    [Required(ErrorMessage = "معرف وحدة البيع مطلوب")]
    public Guid SalesUnitId { get; set; }

    /// <summary>
    /// اسم وحدة البيع (اختياري، في حال عدم تمريره يتم جلبه من وحدة البيع)
    /// </summary>
    [StringLength(SalesUnitConsts.MaxNameLength)]
    public string? UnitName { get; set; }

    /// <summary>
    /// سعر البيع الخاص بهذه الوحدة (اختياري)
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون السعر أكبر من صفر")]
    public decimal? Price { get; set; }

    /// <summary>
    /// هل تكون الوحدة الافتراضية؟
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// تفعيل الوحدة
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ترتيب الظهور
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    #endregion
}

/// <summary>
/// كائن تعديل وحدة بيع المنتج
/// </summary>
public class UpdateProductSalesUnitDto
{
    #region Properties

    /// <summary>
    /// معرف السجل
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// معرف وحدة البيع الأصلية
    /// </summary>
    [Required(ErrorMessage = "معرف وحدة البيع مطلوب")]
    public Guid SalesUnitId { get; set; }

    /// <summary>
    /// اسم وحدة البيع
    /// </summary>
    [StringLength(SalesUnitConsts.MaxNameLength)]
    public string? UnitName { get; set; }

    /// <summary>
    /// سعر البيع الخاص بهذه الوحدة
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون السعر أكبر من صفر")]
    public decimal? Price { get; set; }

    /// <summary>
    /// هل تكون الوحدة الافتراضية؟
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// تفعيل الوحدة
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// ترتيب الظهور
    /// </summary>
    public int DisplayOrder { get; set; }

    #endregion
}
