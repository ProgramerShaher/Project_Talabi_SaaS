using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Payments.Dtos;

/// <summary>
/// كائن طريقة الدفع
/// </summary>
public class PaymentMethodDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public bool IsOnline { get; set; }
    public bool RequiresReceipt { get; set; }
    public bool IsActive { get; set; }
    #endregion
}

/// <summary>
/// كائن عملية الدفع
/// </summary>
public class PaymentDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public Guid OrderId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = PaymentConsts.DefaultCurrency;
    public PaymentTransactionStatus Status { get; set; }
    public string? TransactionReference { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundAmount { get; set; }
    public PaymentReceiptDto? Receipt { get; set; }
    #endregion
}

/// <summary>
/// كائن إيصال الدفع
/// </summary>
public class PaymentReceiptDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public Guid PaymentId { get; set; }
    public Guid MediaFileId { get; set; }
    public string? ReceiptFileUrl { get; set; }
    public string? WalletName { get; set; }
    public string? TransactionNumber { get; set; }
    public decimal? Amount { get; set; }
    public ReceiptVerificationStatus VerificationStatus { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime UploadedAt { get; set; }
    #endregion
}

/// <summary>
/// كائن تسجيل إيصال دفع جديد
/// </summary>
public class SubmitPaymentReceiptInput
{
    #region Properties
    [Required(ErrorMessage = "معرف عملية الدفع مطلوب")]
    public Guid PaymentId { get; set; }

    [Required(ErrorMessage = "معرف ملف الإيصال المرفوع مطلوب")]
    public Guid MediaFileId { get; set; }

    [StringLength(PaymentReceiptConsts.MaxWalletNameLength)]
    public string? WalletName { get; set; }

    [StringLength(PaymentReceiptConsts.MaxTransactionNumberLength)]
    public string? TransactionNumber { get; set; }

    public decimal? Amount { get; set; }
    #endregion
}

/// <summary>
/// كائن مراجعة والتحقق من إيصال الدفع
/// </summary>
public class VerifyPaymentReceiptInput
{
    #region Properties
    [Required(ErrorMessage = "معرف الإيصال مطلوب")]
    public Guid ReceiptId { get; set; }

    public bool IsApproved { get; set; }

    [StringLength(PaymentReceiptConsts.MaxRejectionReasonLength)]
    public string? RejectionReason { get; set; }
    #endregion
}
