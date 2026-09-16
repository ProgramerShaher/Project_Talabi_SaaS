using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Products.Dtos;

/// <summary>
/// كائن عرض بيانات وحدة البيع المستقلة في النظام
/// </summary>
public class SalesUnitDto : FullAuditedEntityDto<Guid>
{
    #region Properties

    /// <summary>
    /// معرف المستأجر
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// اسم وحدة البيع (مثال: كيلو، كيس، حبة، لتر)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// رمز اختصار الوحدة (مثال: KG, Bag, PCS)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// وصف توضيحي للوحدة
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// هل وحدة البيع مفعلة؟
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// ترتيب الظهور
    /// </summary>
    public int DisplayOrder { get; set; }

    #endregion
}

/// <summary>
/// كائن إضافة وحدة بيع جديدة في النظام
/// </summary>
public class CreateSalesUnitDto
{
    #region Properties

    /// <summary>
    /// اسم وحدة البيع
    /// </summary>
    [Required(ErrorMessage = "اسم وحدة البيع مطلوب")]
    [StringLength(SalesUnitConsts.MaxNameLength, ErrorMessage = "تجاوزت الحد الأقصى لاسم وحدة البيع")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// رمز أو كود الوحدة
    /// </summary>
    [StringLength(SalesUnitConsts.MaxCodeLength, ErrorMessage = "تجاوزت الحد الأقصى لكود وحدة البيع")]
    public string? Code { get; set; }

    /// <summary>
    /// وصف الوحدة
    /// </summary>
    [StringLength(SalesUnitConsts.MaxDescriptionLength, ErrorMessage = "تجاوزت الحد الأقصى لوصف وحدة البيع")]
    public string? Description { get; set; }

    /// <summary>
    /// ترتيب العرض
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// تفعيل الوحدة
    /// </summary>
    public bool IsActive { get; set; } = true;

    #endregion
}

/// <summary>
/// كائن تعديل بيانات وحدة البيع
/// </summary>
public class UpdateSalesUnitDto
{
    #region Properties

    /// <summary>
    /// اسم وحدة البيع
    /// </summary>
    [Required(ErrorMessage = "اسم وحدة البيع مطلوب")]
    [StringLength(SalesUnitConsts.MaxNameLength, ErrorMessage = "تجاوزت الحد الأقصى لاسم وحدة البيع")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// رمز أو كود الوحدة
    /// </summary>
    [StringLength(SalesUnitConsts.MaxCodeLength, ErrorMessage = "تجاوزت الحد الأقصى لكود وحدة البيع")]
    public string? Code { get; set; }

    /// <summary>
    /// وصف الوحدة
    /// </summary>
    [StringLength(SalesUnitConsts.MaxDescriptionLength, ErrorMessage = "تجاوزت الحد الأقصى لوصف وحدة البيع")]
    public string? Description { get; set; }

    /// <summary>
    /// ترتيب العرض
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// تفعيل الوحدة
    /// </summary>
    public bool IsActive { get; set; }

    #endregion
}

/// <summary>
/// مدخلات البحث والفلترة لقائمة وحدات البيع
/// </summary>
public class GetSalesUnitListInput : PagedAndSortedResultRequestDto
{
    #region Properties

    /// <summary>
    /// نص البحث
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// فلترة حسب حالة التفعيل
    /// </summary>
    public bool? IsActive { get; set; }

    #endregion
}
