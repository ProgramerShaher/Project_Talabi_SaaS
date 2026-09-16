using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Deliveries.Dtos;

/// <summary>
/// كائن عرض بيانات مندوب التوصيل
/// </summary>
public class CourierDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public Guid UserId { get; set; }
    public string? VehicleType { get; set; }
    public string? VehicleNumber { get; set; }
    public string? LicenseNumber { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsOnline { get; set; }
    public decimal? CurrentLatitude { get; set; }
    public decimal? CurrentLongitude { get; set; }
    public DateTime? LastLocationUpdate { get; set; }
    // public decimal Rating { get; set; }
    public int TotalDeliveries { get; set; }
    // public decimal TotalEarnings { get; set; }
    #endregion
}

/// <summary>
/// كائن تسجيل مندوب توصيل جديد
/// </summary>
public class CreateCourierDto
{
    #region Properties
    [Required(ErrorMessage = "معرف المستخدم مطلوب")]
    public Guid UserId { get; set; }

    [StringLength(CourierConsts.MaxVehicleTypeLength)]
    public string? VehicleType { get; set; }

    [StringLength(CourierConsts.MaxVehicleNumberLength)]
    public string? VehicleNumber { get; set; }

    [StringLength(CourierConsts.MaxLicenseNumberLength)]
    public string? LicenseNumber { get; set; }
    #endregion
}

/// <summary>
/// كائن تحديث موقع المندوب اللحظي
/// </summary>
public class UpdateCourierLocationInput
{
    #region Properties
    [Required]
    public decimal Latitude { get; set; }

    [Required]
    public decimal Longitude { get; set; }
    #endregion
}

/// <summary>
/// كائن عملية تعيين التوصيل
/// </summary>
public class DeliveryAssignmentDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public Guid OrderId { get; set; }
    public Guid CourierId { get; set; }
    public string? CourierName { get; set; }
    public DeliveryAssignmentStatus Status { get; set; }
    public Guid AssignedByUserId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? PickupAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? EstimatedArrival { get; set; }
    public decimal? DistanceKm { get; set; }
    // public decimal? DeliveryFee { get; set; }
    // public decimal? CourierEarning { get; set; }
    public DeliveryConfirmationDto? Confirmation { get; set; }
    #endregion
}

/// <summary>
/// كائن إسناد وتعيين طلب لمندوب
/// </summary>
public class AssignCourierInput
{
    #region Properties
    [Required(ErrorMessage = "معرف الطلب مطلوب")]
    public Guid OrderId { get; set; }

    [Required(ErrorMessage = "معرف المندوب مطلوب")]
    public Guid CourierId { get; set; }
    #endregion
}

/// <summary>
/// كائن تأكيد تسليم الطلب
/// </summary>
public class DeliveryConfirmationDto : CreationAuditedEntityDto<Guid>
{
    #region Properties
    public Guid DeliveryAssignmentId { get; set; }
    public DeliveryConfirmedBy ConfirmedBy { get; set; }
    public Guid ConfirmedByUserId { get; set; }
    public string? VerificationCode { get; set; }
    public string? SignatureUrl { get; set; }
    public string? PhotoProofUrl { get; set; }
    public string? Notes { get; set; }
    public DateTime ConfirmedAt { get; set; }
    #endregion
}

/// <summary>
/// كائن طلب تأكيد وإثبات التسليم
/// </summary>
public class ConfirmDeliveryInput
{
    #region Properties
    [Required(ErrorMessage = "معرف تعيين التوصيل مطلوب")]
    public Guid DeliveryAssignmentId { get; set; }

    public DeliveryConfirmedBy ConfirmedBy { get; set; }

    [StringLength(DeliveryConfirmationConsts.MaxVerificationCodeLength)]
    public string? VerificationCode { get; set; }

    [StringLength(DeliveryConfirmationConsts.MaxSignatureUrlLength)]
    public string? SignatureUrl { get; set; }

    [StringLength(DeliveryConfirmationConsts.MaxPhotoProofUrlLength)]
    public string? PhotoProofUrl { get; set; }

    [StringLength(DeliveryConfirmationConsts.MaxNotesLength)]
    public string? Notes { get; set; }
    #endregion
}

/// <summary>
/// كائن تعديل بيانات مندوب التوصيل
/// </summary>
public class UpdateCourierDto
{
    #region Properties
    [StringLength(CourierConsts.MaxVehicleTypeLength)]
    public string? VehicleType { get; set; }

    [StringLength(CourierConsts.MaxVehicleNumberLength)]
    public string? VehicleNumber { get; set; }

    [StringLength(CourierConsts.MaxLicenseNumberLength)]
    public string? LicenseNumber { get; set; }

    public bool? IsAvailable { get; set; }
    #endregion
}

/// <summary>
/// كائن فلترة وبحث قائمة المناديب
/// </summary>
public class GetCourierListInput : PagedAndSortedResultRequestDto
{
    #region Properties
    public string? Filter { get; set; }
    public bool? IsAvailable { get; set; }
    public bool? IsOnline { get; set; }
    public string? VehicleType { get; set; }
    #endregion
}

/// <summary>
/// كائن استعلام مشاوير المندوب الحالي
/// </summary>
public class GetMyDeliveriesInput : PagedAndSortedResultRequestDto
{
    #region Properties
    public DeliveryAssignmentStatus? Status { get; set; }
    #endregion
}

/// <summary>
/// كائن تسجيل فشل توصيل الطلب
/// </summary>
public class FailDeliveryInput
{
    #region Properties
    [Required(ErrorMessage = "سبب تعثر وفشل التوصيل مطلوب")]
    [StringLength(DeliveryConfirmationConsts.MaxNotesLength)]
    public string Reason { get; set; } = string.Empty;
    #endregion
}
