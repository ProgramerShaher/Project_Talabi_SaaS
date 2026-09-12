using System;
using System.Collections.Generic;
using Talabi.Customers;
using Talabi.Stores;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Carts;

/// <summary>
/// كيان سلة المشتريات للعميل
/// </summary>
public class Cart : FullAuditedAggregateRoot<Guid>
{
    /// <summary>
    /// معرف العميل صاحب السلة
    /// </summary>
    public virtual Guid CustomerId { get; set; }

    /// <summary>
    /// معرف المتجر الذي تنتمي إليه منتجات السلة
    /// </summary>
    public virtual Guid StoreId { get; set; }

    /// <summary>
    /// هل السلة نشطة ومفتوحة حالياً؟
    /// </summary>
    public virtual bool IsActive { get; set; } = true;

    /// <summary>
    /// تاريخ ووقت انتهاء صلاحية السلة التلقائي
    /// </summary>
    public virtual DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// تاريخ ووقت آخر نشاط أو إضافة في السلة
    /// </summary>
    public virtual DateTime LastActivityAt { get; set; }

    /// <summary>
    /// كائن العميل المرتبط
    /// </summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// كائن المتجر المرتبط
    /// </summary>
    public virtual Store? Store { get; set; }

    /// <summary>
    /// عناصر ومنتجات السلة
    /// </summary>
    public virtual ICollection<CartItem> Items { get; protected set; } = new List<CartItem>();

    protected Cart()
    {
    }

    public Cart(Guid id, Guid customerId, Guid storeId, DateTime? expiresAt = null)
        : base(id)
    {
        CustomerId = customerId;
        StoreId = storeId;
        ExpiresAt = expiresAt;
        IsActive = true;
        LastActivityAt = DateTime.UtcNow;
    }
}
