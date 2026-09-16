using System;
using Talabi.Orders;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Payments;

/// <summary>
/// كيان عملية الدفع المالية للطلب (Payment Aggregate Root)
/// </summary>
public class Payment : FullAuditedAggregateRoot<Guid>
{
    /// <summary>
    /// معرف الطلب المرتبط (علاقة 1:1 فريدة مع الطلب)
    /// </summary>
    public virtual Guid OrderId { get; set; }

    /// <summary>
    /// معرف طريقة الدفع المختارة
    /// </summary>
    public virtual Guid PaymentMethodId { get; set; }

    /// <summary>
    /// المبلغ المطلوب دفعه
    /// </summary>
    public virtual decimal Amount { get; set; }

    /// <summary>
    /// عملة الدفع (الافتراضي: SAR)
    /// </summary>
    public virtual string Currency { get; set; } = PaymentConsts.DefaultCurrency;

    /// <summary>
    /// حالة عملية الدفع (قيد الانتظار، مكتملة، فاشلة، مسترجعة)
    /// </summary>
    public virtual PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Pending;

    /// <summary>
    /// الرقم المرجعي للعملية من بوابة الدفع أو البنك
    /// </summary>
    public virtual string? TransactionReference { get; set; }

    /// <summary>
    /// الاستجابة الكاملة من بوابة الدفع بتنسيق JSON للتدقيق
    /// </summary>
    public virtual string? GatewayResponse { get; set; }

    /// <summary>
    /// تاريخ ووقت نجاح عملية الدفع
    /// </summary>
    public virtual DateTime? PaidAt { get; set; }

    /// <summary>
    /// تاريخ ووقت استرجاع المبلغ
    /// </summary>
    public virtual DateTime? RefundedAt { get; set; }

    /// <summary>
    /// قيمة المبلغ المسترجع
    /// </summary>
    public virtual decimal? RefundAmount { get; set; }

    /// <summary>
    /// الطلب المرتبط
    /// </summary>
    public virtual Order? Order { get; set; }

    /// <summary>
    /// طريقة الدفع المرتبطة
    /// </summary>
    public virtual PaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// إيصال التحويل المرتبط في حال التحويل البنكي أو المحافظ
    /// </summary>
    public virtual PaymentReceipt? Receipt { get; set; }

    protected Payment()
    {
    }

    public Payment(
        Guid id,
        Guid orderId,
        Guid paymentMethodId,
        decimal amount,
        string currency = PaymentConsts.DefaultCurrency)
        : base(id)
    {
        OrderId = orderId;
        PaymentMethodId = paymentMethodId;
        Amount = amount;
        Currency = currency;
        Status = PaymentTransactionStatus.Pending;
    }

    /// <summary>
    /// وسم المعاملة كمكتملة وناجحة
    /// </summary>
    public void MarkAsCompleted(string? transactionReference = null)
    {
        Status = PaymentTransactionStatus.Completed;
        PaidAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(transactionReference))
        {
            TransactionReference = transactionReference;
        }
    }

    /// <summary>
    /// وسم المعاملة كفاشلة
    /// </summary>
    public void MarkAsFailed()
    {
        Status = PaymentTransactionStatus.Failed;
    }

    /// <summary>
    /// وسم المعاملة كمسترجعة
    /// </summary>
    public void MarkAsRefunded(decimal? refundAmount = null)
    {
        Status = PaymentTransactionStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
        RefundAmount = refundAmount ?? Amount;
    }
}
