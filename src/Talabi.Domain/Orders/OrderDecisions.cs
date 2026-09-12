using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Orders;

/// <summary>
/// كيان توثيق رفض الطلب (من قِبل المتجر مثلاً)
/// </summary>
public class OrderRejection : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// معرف الطلب المرفوض (علاقة 1:1 فريدة مع الطلب)
    /// </summary>
    public virtual Guid OrderId { get; set; }

    /// <summary>
    /// معرف المستخدم الذي قام برفض الطلب
    /// </summary>
    public virtual Guid RejectedByUserId { get; set; }

    /// <summary>
    /// معرف سبب الرفض المعياري
    /// </summary>
    public virtual Guid RejectionReasonId { get; set; }

    /// <summary>
    /// ملاحظات أو تبريرات إضافية
    /// </summary>
    public virtual string? AdditionalNotes { get; set; }

    /// <summary>
    /// وقت وتاريخ الرفض
    /// </summary>
    public virtual DateTime RejectedAt { get; set; }

    /// <summary>
    /// الطلب المرتبط
    /// </summary>
    public virtual Order? Order { get; set; }

    /// <summary>
    /// سبب الرفض المرتبط
    /// </summary>
    public virtual CancellationReason? RejectionReason { get; set; }

    protected OrderRejection()
    {
    }

    public OrderRejection(
        Guid id,
        Guid orderId,
        Guid rejectedByUserId,
        Guid rejectionReasonId,
        string? additionalNotes = null)
        : base(id)
    {
        OrderId = orderId;
        RejectedByUserId = rejectedByUserId;
        RejectionReasonId = rejectionReasonId;
        AdditionalNotes = additionalNotes;
        RejectedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// كيان توثيق إلغاء الطلب (من قِبل العميل أو الإدارة)
/// </summary>
public class OrderCancellation : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// معرف الطلب الملغى (علاقة 1:1 فريدة مع الطلب)
    /// </summary>
    public virtual Guid OrderId { get; set; }

    /// <summary>
    /// معرف المستخدم الذي قام بالإلغاء
    /// </summary>
    public virtual Guid CancelledByUserId { get; set; }

    /// <summary>
    /// معرف سبب الإلغاء المعياري
    /// </summary>
    public virtual Guid CancellationReasonId { get; set; }

    /// <summary>
    /// صفة القائم بالإلغاء (Customer, Store, Admin)
    /// </summary>
    public virtual string CancelledByRole { get; set; } = string.Empty;

    /// <summary>
    /// معرف الحالة السابقة التي كان عليها الطلب قبل الإلغاء
    /// </summary>
    public virtual Guid PreviousStatusId { get; set; }

    /// <summary>
    /// ملاحظات توضيحية إضافية
    /// </summary>
    public virtual string? AdditionalNotes { get; set; }

    /// <summary>
    /// وقت وتاريخ الإلغاء
    /// </summary>
    public virtual DateTime CancelledAt { get; set; }

    /// <summary>
    /// الطلب المرتبط
    /// </summary>
    public virtual Order? Order { get; set; }

    /// <summary>
    /// سبب الإلغاء المرتبط
    /// </summary>
    public virtual CancellationReason? CancellationReason { get; set; }

    /// <summary>
    /// الحالة السابقة المرتبطة
    /// </summary>
    public virtual OrderStatus? PreviousStatus { get; set; }

    protected OrderCancellation()
    {
    }

    public OrderCancellation(
        Guid id,
        Guid orderId,
        Guid cancelledByUserId,
        Guid cancellationReasonId,
        string cancelledByRole,
        Guid previousStatusId,
        string? additionalNotes = null)
        : base(id)
    {
        OrderId = orderId;
        CancelledByUserId = cancelledByUserId;
        CancellationReasonId = cancellationReasonId;
        CancelledByRole = cancelledByRole;
        PreviousStatusId = previousStatusId;
        AdditionalNotes = additionalNotes;
        CancelledAt = DateTime.UtcNow;
    }
}
