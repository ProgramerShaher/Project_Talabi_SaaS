using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Orders.Dtos;

/// <summary>
/// كائن عرض حالة الطلب
/// </summary>
public class OrderStatusDto : EntityDto<Guid>
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFinal { get; set; }
    #endregion
}

/// <summary>
/// كائن سجل تتبع تغير حالة الطلب
/// </summary>
public class OrderStatusHistoryDto : CreationAuditedEntityDto<Guid>
{
    #region Properties
    public Guid OrderId { get; set; }
    public Guid? FromStatusId { get; set; }
    public string? FromStatusName { get; set; }
    public Guid ToStatusId { get; set; }
    public string ToStatusName { get; set; } = string.Empty;
    public Guid ChangedByUserId { get; set; }
    public string ChangedByRole { get; set; } = string.Empty;
    public string? Notes { get; set; }
    #endregion
}

/// <summary>
/// كائن سبب الإلغاء أو الرفض
/// </summary>
public class CancellationReasonDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public string Reason { get; set; } = string.Empty;
    public CancellationTargetAudience TargetAudience { get; set; }
    public bool IsActive { get; set; }
    #endregion
}

/// <summary>
/// كائن طلب رفض الطلب من قِبل المتجر
/// </summary>
public class RejectOrderInput
{
    #region Properties
    [Required(ErrorMessage = "معرف الطلب مطلوب")]
    public Guid OrderId { get; set; }

    [Required(ErrorMessage = "معرف سبب الرفض مطلوب")]
    public Guid RejectionReasonId { get; set; }

    [StringLength(CancellationReasonConsts.MaxAdditionalNotesLength)]
    public string? AdditionalNotes { get; set; }
    #endregion
}

/// <summary>
/// كائن طلب إلغاء الطلب من قِبل العميل أو الإدارة
/// </summary>
public class CancelOrderInput
{
    #region Properties
    [Required(ErrorMessage = "معرف الطلب مطلوب")]
    public Guid OrderId { get; set; }

    [Required(ErrorMessage = "معرف سبب الإلغاء مطلوب")]
    public Guid CancellationReasonId { get; set; }

    [StringLength(CancellationReasonConsts.MaxAdditionalNotesLength)]
    public string? AdditionalNotes { get; set; }
    #endregion
}
