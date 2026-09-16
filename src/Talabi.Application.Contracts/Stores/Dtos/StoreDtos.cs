using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Stores.Dtos;

/// <summary>
/// كائن عرض بيانات المتجر
/// </summary>
public class StoreDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public Guid? TenantId { get; set; }
    public Guid OwnerId { get; set; }
    public Guid StoreTypeId { get; set; }
    public string StoreTypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? WorkingHoursJson { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    // public decimal DeliveryFee { get; set; }
    public int? AverageDeliveryTime { get; set; }
    public StoreStatus Status { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public decimal Rating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalOrders { get; set; }
    /// <summary>
    /// هل المتجر مفتوح حالياً ومتاح لاستقبال الطلبات
    /// </summary>
    public bool IsOpenNow { get; set; }
    #endregion
}

/// <summary>
/// كائن إنشاء متجر جديد
/// </summary>
public class CreateStoreDto
{
    #region Properties
    [Required(ErrorMessage = "معرف مالك المتجر مطلوب")]
    public Guid OwnerId { get; set; }

    [Required(ErrorMessage = "معرف نوع المتجر مطلوب")]
    public Guid StoreTypeId { get; set; }

    [Required(ErrorMessage = "اسم المتجر مطلوب")]
    [StringLength(StoreConsts.MaxNameLength, ErrorMessage = "تجاوزت الحد الأقصى لاسم المتجر")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "الرابط اللطيف للمتجر مطلوب")]
    [StringLength(StoreConsts.MaxSlugLength, ErrorMessage = "تجاوزت الحد الأقصى للرابط")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(StoreConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [StringLength(StoreConsts.MaxLogoUrlLength)]
    public string? LogoUrl { get; set; }

    [StringLength(StoreConsts.MaxCoverImageUrlLength)]
    public string? CoverImageUrl { get; set; }

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [StringLength(StoreConsts.MaxPhoneLength, ErrorMessage = "تجاوزت الحد الأقصى لرقم الهاتف")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    [StringLength(StoreConsts.MaxEmailLength)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "عنوان المتجر مطلوب")]
    [StringLength(StoreConsts.MaxAddressLength)]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "خط العرض مطلوب")]
    public decimal Latitude { get; set; }

    [Required(ErrorMessage = "خط الطول مطلوب")]
    public decimal Longitude { get; set; }

    public string? WorkingHoursJson { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    // public decimal DeliveryFee { get; set; }
    public int? AverageDeliveryTime { get; set; }
    #endregion
}

/// <summary>
/// كائن تعديل بيانات متجر
/// </summary>
public class UpdateStoreDto
{
    #region Properties
    [Required(ErrorMessage = "معرف نوع المتجر مطلوب")]
    public Guid StoreTypeId { get; set; }

    [Required(ErrorMessage = "اسم المتجر مطلوب")]
    [StringLength(StoreConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(StoreConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [StringLength(StoreConsts.MaxLogoUrlLength)]
    public string? LogoUrl { get; set; }

    [StringLength(StoreConsts.MaxCoverImageUrlLength)]
    public string? CoverImageUrl { get; set; }

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [StringLength(StoreConsts.MaxPhoneLength)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(StoreConsts.MaxEmailLength)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "العنوان مطلوب")]
    [StringLength(StoreConsts.MaxAddressLength)]
    public string Address { get; set; } = string.Empty;

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? WorkingHoursJson { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    // public decimal DeliveryFee { get; set; }
    public int? AverageDeliveryTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    #endregion
}

/// <summary>
/// كائن عرض حساب دفع للمتجر
/// </summary>
public class StorePaymentAccountDto : FullAuditedEntityDto<Guid>
{
    public Guid StoreId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// كائن إنشاء حساب دفع جديد للمتجر
/// </summary>
public class CreateStorePaymentAccountDto
{
    [Required(ErrorMessage = "معرف المتجر مطلوب")]
    public Guid StoreId { get; set; }

    [Required(ErrorMessage = "اسم مزود الخدمة مطلوب (مثل: جوالي، الكريمي)")]
    [StringLength(64)]
    public string ProviderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم الحساب أو رقم الهاتف مطلوب")]
    [StringLength(64)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم صاحب الحساب مطلوب للمطابقة")]
    [StringLength(128)]
    public string AccountName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [StringLength(512)]
    public string? Notes { get; set; }
}

/// <summary>
/// فترة عمل داخل اليوم
/// </summary>
public class WorkingHoursShiftDto
{
    /// <summary>
    /// وقت الفتح بتنسيق 24 ساعة HH:mm (مثل: 08:00)
    /// </summary>
    [Required(ErrorMessage = "وقت بدء فترة العمل مطلوب")]
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "صيغة وقت الفتح يجب أن تكون HH:mm")]
    public string OpeningTime { get; set; } = "08:00";

    /// <summary>
    /// وقت الإغلاق بتنسيق 24 ساعة HH:mm (مثل: 23:00)
    /// </summary>
    [Required(ErrorMessage = "وقت انتهاء فترة العمل مطلوب")]
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "صيغة وقت الإغلاق يجب أن تكون HH:mm")]
    public string ClosingTime { get; set; } = "23:00";
}

/// <summary>
/// بيانات ساعات عمل يوم محدد في الأسبوع
/// </summary>
public class StoreWorkingDayDto
{
    /// <summary>
    /// يوم الأسبوع (Sunday = 0, Monday = 1, ...)
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// اسم اليوم باللغة العربية (الأحد، الإثنين، ...)
    /// </summary>
    public string DayName { get; set; } = string.Empty;

    /// <summary>
    /// هل المتجر يعمل في هذا اليوم أم مغلق (عطلة أسبوعية)
    /// </summary>
    public bool IsOpen { get; set; } = true;

    /// <summary>
    /// فترات العمل في اليوم (صباحية، مسائية...)
    /// </summary>
    public List<WorkingHoursShiftDto> Shifts { get; set; } = new();
}

/// <summary>
/// كائن إدخال وتعديل جدول ساعات وأيام العمل الأسبوعية للمتجر
/// </summary>
public class SetStoreWorkingHoursInput
{
    /// <summary>
    /// معرف المتجر
    /// </summary>
    [Required(ErrorMessage = "معرف المتجر مطلوب")]
    public Guid StoreId { get; set; }

    /// <summary>
    /// قائمة أيام الأسبوع السبعة مع فترات وساعات العمل
    /// </summary>
    public List<StoreWorkingDayDto> Days { get; set; } = new();

    /// <summary>
    /// المنطقة الزمنية المعتمدة (الافتراضي: Asia/Riyadh)
    /// </summary>
    public string? TimeZone { get; set; } = "Asia/Riyadh";
}

/// <summary>
/// كائن عرض جدول ساعات عمل المتجر وحالته اللحظية
/// </summary>
public class StoreWorkingHoursDto
{
    /// <summary>
    /// معرف المتجر
    /// </summary>
    public Guid StoreId { get; set; }

    /// <summary>
    /// اسم المتجر
    /// </summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// جدول الأيام السبعة وفترات العمل
    /// </summary>
    public List<StoreWorkingDayDto> Days { get; set; } = new();

    /// <summary>
    /// المنطقة الزمنية
    /// </summary>
    public string? TimeZone { get; set; } = "Asia/Riyadh";

    /// <summary>
    /// هل المتجر مفتوح في هذه اللحظة؟
    /// </summary>
    public bool IsOpenNow { get; set; }

    /// <summary>
    /// رسالة نصية توضح حالة المتجر
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;
}

/// <summary>
/// كائن فحص حالة المتجر الآن
/// </summary>
public class StoreOpenStatusDto
{
    /// <summary>
    /// معرف المتجر
    /// </summary>
    public Guid StoreId { get; set; }

    /// <summary>
    /// اسم المتجر
    /// </summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// هل المتجر مفتوح ومتاح لاستقبال الطلبات الآن
    /// </summary>
    public bool IsOpenNow { get; set; }

    /// <summary>
    /// هل المتجر مفعل تشغيلياً من التاجر
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// حالة المتجر في المنصة
    /// </summary>
    public StoreStatus Status { get; set; }

    /// <summary>
    /// رسالة الحالة الموجهة للمستخدم
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// الموعد القادم لفتح المتجر إن كان مغلقاً
    /// </summary>
    public string? NextOpenTime { get; set; }
}

/// <summary>
/// كائن مدخلات تعليق المتجر من قبل الإدارة
/// </summary>
public class SuspendStoreInput
{
    /// <summary>
    /// سبب تعليق نشاط المتجر
    /// </summary>
    [Required(ErrorMessage = "يرجى توضيح سبب تعليق المتجر")]
    [StringLength(500, ErrorMessage = "سبب التعليق لا يتجاوز 500 حرف")]
    public string Reason { get; set; } = string.Empty;
}
