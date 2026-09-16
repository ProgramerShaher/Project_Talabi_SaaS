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
/// كائن تفاصيل إيصال الدفع الموسعة متضمناً بيانات الطلب والمتجر والعميل
/// </summary>
public class PaymentReceiptDetailsDto : PaymentReceiptDto
{
    #region Extended Properties
    /// <summary>
    /// معرف الطلب المرتبط
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// رقم الطلب التسلسلي (مثل: ORD-20260916-1234)
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// المبلغ الإجمالي للطلب
    /// </summary>
    public decimal OrderFinalAmount { get; set; }

    /// <summary>
    /// معرف المتجر
    /// </summary>
    public Guid StoreId { get; set; }

    /// <summary>
    /// اسم المتجر
    /// </summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// معرف العميل صاحب الطلب
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// اسم العميل
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// رقم هاتف العميل
    /// </summary>
    public string? CustomerPhoneNumber { get; set; }

    /// <summary>
    /// اسم طريقة الدفع المختارة
    /// </summary>
    public string PaymentMethodDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// اسم المستخدم أو المشرف الذي راجع الإيصال
    /// </summary>
    public string? VerifiedByUserName { get; set; }
    #endregion
}

/// <summary>
/// كائن استعلام وقائمة إيصالات الدفع
/// </summary>
public class GetPaymentReceiptListInput : PagedAndSortedResultRequestDto
{
    #region Properties
    /// <summary>
    /// تصفية حسب المتجر
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// تصفية حسب الطلب
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// تصفية حسب حالة التحقق
    /// </summary>
    public ReceiptVerificationStatus? Status { get; set; }

    /// <summary>
    /// تصفية حسب تاريخ البداية
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// تصفية حسب تاريخ النهاية
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// نص البحث (رقم الطلب، رقم الحوالة، اسم المحفظة)
    /// </summary>
    public string? Filter { get; set; }
    #endregion
}

/// <summary>
/// كائن تسجيل إيصال دفع جديد
/// </summary>
public class SubmitPaymentReceiptInput
{
    #region Properties
    /// <summary>
    /// معرف عملية الدفع (اختياري إذا تم تحديد معرف الطلب)
    /// </summary>
    public Guid? PaymentId { get; set; }

    /// <summary>
    /// معرف الطلب المرتبط (اختياري إذا تم تحديد معرف عملية الدفع)
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// معرف ملف الإيصال المرفوع
    /// </summary>
    [Required(ErrorMessage = "معرف ملف الإيصال المرفوع مطلوب")]
    public Guid MediaFileId { get; set; }

    /// <summary>
    /// اسم المحفظة أو البنك المحول منه
    /// </summary>
    [StringLength(PaymentReceiptConsts.MaxWalletNameLength)]
    public string? WalletName { get; set; }

    /// <summary>
    /// رقم الحوالة أو العملية
    /// </summary>
    [StringLength(PaymentReceiptConsts.MaxTransactionNumberLength)]
    public string? TransactionNumber { get; set; }

    /// <summary>
    /// المبلغ المذكور في الإيصال
    /// </summary>
    public decimal? Amount { get; set; }
    #endregion
}

/// <summary>
/// كائن مراجعة والتحقق من إيصال الدفع
/// </summary>
public class VerifyPaymentReceiptInput
{
    #region Properties
    /// <summary>
    /// معرف الإيصال
    /// </summary>
    [Required(ErrorMessage = "معرف الإيصال مطلوب")]
    public Guid ReceiptId { get; set; }

    /// <summary>
    /// هل تم قبول واعتماد الإيصال؟
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// سبب الرفض في حال عدم القبول
    /// </summary>
    [StringLength(PaymentReceiptConsts.MaxRejectionReasonLength)]
    public string? RejectionReason { get; set; }
    #endregion
}
