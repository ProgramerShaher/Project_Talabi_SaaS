using System;
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
