using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Payments;

/// <summary>
/// كيان إيصال التحويل المالي أو المحفظة الإلكترونية للتحقق اليدوي
/// </summary>
public class PaymentReceipt : FullAuditedEntity<Guid>
{
    /// <summary>
    /// معرف عملية الدفع المرتبطة (علاقة 1:1 فريدة)
    /// </summary>
    public virtual Guid PaymentId { get; set; }

    /// <summary>
    /// معرف ملف صورة الإيصال في جدول الملفات والوسائط (MediaFiles)
    /// </summary>
    public virtual Guid MediaFileId { get; set; }

    /// <summary>
    /// اسم المحفظة أو البنك المحول منه (مثل: STC Pay, Urpay, الراجحي)
    /// </summary>
    public virtual string? WalletName { get; set; }

    /// <summary>
    /// رقم الحوالة أو العملية المسجل في الإيصال
    /// </summary>
    public virtual string? TransactionNumber { get; set; }

    /// <summary>
    /// المبلغ المكتوب في الإيصال
    /// </summary>
    public virtual decimal? Amount { get; set; }

    /// <summary>
    /// حالة التحقق من الإيصال (قيد الانتظار، مقبول، مرفوض)
    /// </summary>
    public virtual ReceiptVerificationStatus VerificationStatus { get; set; } = ReceiptVerificationStatus.Pending;

    /// <summary>
    /// معرف الموظف أو المدير الذي راجع الإيصال
    /// </summary>
    public virtual Guid? VerifiedByUserId { get; set; }

    /// <summary>
    /// تاريخ ووقت التحقق والمراجعة
    /// </summary>
    public virtual DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// سبب رفض الإيصال في حال الرفض
    /// </summary>
    public virtual string? RejectionReason { get; set; }

    /// <summary>
    /// تاريخ ووقت رفع الإيصال من قِبل العميل
    /// </summary>
    public virtual DateTime UploadedAt { get; set; }

    /// <summary>
    /// كائن الدفع المرتبط
    /// </summary>
    public virtual Payment? Payment { get; set; }

    protected PaymentReceipt()
    {
    }

    public PaymentReceipt(
        Guid id,
        Guid paymentId,
        Guid mediaFileId,
        string? walletName = null,
        string? transactionNumber = null,
        decimal? amount = null)
        : base(id)
    {
        PaymentId = paymentId;
        MediaFileId = mediaFileId;
        WalletName = walletName;
        TransactionNumber = transactionNumber;
        Amount = amount;
        VerificationStatus = ReceiptVerificationStatus.Pending;
        UploadedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// قبول واعتماد إيصال الدفع
    /// </summary>
    public void Verify(Guid verifiedByUserId)
    {
        VerificationStatus = ReceiptVerificationStatus.Verified;
        VerifiedByUserId = verifiedByUserId;
        VerifiedAt = DateTime.UtcNow;
        RejectionReason = null;
    }

    /// <summary>
    /// رفض إيصال الدفع مع تدوين السبب
    /// </summary>
    public void Reject(Guid verifiedByUserId, string reason)
    {
        VerificationStatus = ReceiptVerificationStatus.Rejected;
        VerifiedByUserId = verifiedByUserId;
        VerifiedAt = DateTime.UtcNow;
        RejectionReason = reason;
    }

    /// <summary>
    /// تحديث بيانات الإيصال وإعادة ضبطه قيد الانتظار عند إعادة الرفع
    /// </summary>
    public void Resubmit(Guid mediaFileId, string? walletName = null, string? transactionNumber = null, decimal? amount = null)
    {
        MediaFileId = mediaFileId;
        if (!string.IsNullOrWhiteSpace(walletName))
        {
            WalletName = walletName;
        }
        if (!string.IsNullOrWhiteSpace(transactionNumber))
        {
            TransactionNumber = transactionNumber;
        }
        if (amount.HasValue)
        {
            Amount = amount.Value;
        }
        VerificationStatus = ReceiptVerificationStatus.Pending;
        RejectionReason = null;
        UploadedAt = DateTime.UtcNow;
        VerifiedAt = null;
        VerifiedByUserId = null;
    }
}
