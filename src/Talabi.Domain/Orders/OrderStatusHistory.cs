using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Orders;

/// <summary>
/// كيان سجل تتبع وتغير حالات الطلب المتعاقبة
/// </summary>
public class OrderStatusHistory : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// معرف الطلب
    /// </summary>
    public virtual Guid OrderId { get; set; }

    /// <summary>
    /// معرف الحالة السابقة (قد تكون فارغة عند أول حالة للطلب)
    /// </summary>
    public virtual Guid? FromStatusId { get; set; }

    /// <summary>
    /// معرف الحالة الجديدة التي انتقل إليها الطلب
    /// </summary>
    public virtual Guid ToStatusId { get; set; }

    /// <summary>
    /// معرف المستخدم الذي قام بتغيير الحالة
    /// </summary>
    public virtual Guid ChangedByUserId { get; set; }

    /// <summary>
    /// صفة أو دور من قام بالتغيير (Customer, Store, Courier, System)
    /// </summary>
    public virtual string ChangedByRole { get; set; } = string.Empty;

    /// <summary>
    /// ملاحظات توضيحية حول التغيير
    /// </summary>
    public virtual string? Notes { get; set; }

    /// <summary>
    /// الطلب المرتبط
    /// </summary>
    public virtual Order? Order { get; set; }

    /// <summary>
    /// كائن الحالة السابقة
    /// </summary>
    public virtual OrderStatus? FromStatus { get; set; }

    /// <summary>
    /// كائن الحالة الجديدة
    /// </summary>
    public virtual OrderStatus? ToStatus { get; set; }

    protected OrderStatusHistory()
    {
    }

    public OrderStatusHistory(
        Guid id,
        Guid orderId,
        Guid toStatusId,
        Guid changedByUserId,
        string changedByRole,
        Guid? fromStatusId = null,
        string? notes = null)
        : base(id)
    {
        OrderId = orderId;
        ToStatusId = toStatusId;
        ChangedByUserId = changedByUserId;
        ChangedByRole = changedByRole;
        FromStatusId = fromStatusId;
        Notes = notes;
    }
}
