using System;
using Talabi.Customers;
using Talabi.Deliveries;
using Talabi.Orders;
using Talabi.Stores;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Interactions;

/// <summary>
/// كيان تقييمات ومراجعات الطلبات والمتاجر والمناديب
/// </summary>
public class Review : FullAuditedEntity<Guid>
{
    /// <summary>
    /// معرف الطلب المقيم (معطل - التقييم للنشاط التجاري/المتجر مباشرة دون ربطه بطلب محدد)
    /// </summary>
    // public virtual Guid OrderId { get; set; }

    /// <summary>
    /// معرف العميل المقيم
    /// </summary>
    public virtual Guid CustomerId { get; set; }

    /// <summary>
    /// معرف المتجر المقيم (النشاط التجاري)
    /// </summary>
    public virtual Guid StoreId { get; set; }

    /// <summary>
    /// معرف المندوب المقيم إن وجد (معطل - التقييم للنشاط التجاري فقط)
    /// </summary>
    // public virtual Guid? CourierId { get; set; }

    /// <summary>
    /// تقييم المتجر والنشاط التجاري (من 1 إلى 5 نجوم)
    /// </summary>
    public virtual int StoreRating { get; set; }

    /// <summary>
    /// تقييم المندوب والتوصيل (معطل - التقييم للنشاط التجاري فقط)
    /// </summary>
    // public virtual int? CourierRating { get; set; }

    /// <summary>
    /// تقييم جودة المنتج (من 1 إلى 5 نجوم)
    /// </summary>
    public virtual int? ProductQualityRating { get; set; }

    /// <summary>
    /// تقييم سرعة التوصيل (معطل - التقييم للنشاط التجاري فقط)
    /// </summary>
    // public virtual int? DeliverySpeedRating { get; set; }

    /// <summary>
    /// نص تعليق وملاحظات العميل
    /// </summary>
    public virtual string? Comment { get; set; }

    /// <summary>
    /// رد المتجر الرسمي على التقييم
    /// </summary>
    public virtual string? StoreReply { get; set; }

    /// <summary>
    /// تاريخ ووقت رد المتجر
    /// </summary>
    public virtual DateTime? StoreRepliedAt { get; set; }

    /// <summary>
    /// هل التقييم مرئي للعامة في شاشة المتجر؟
    /// </summary>
    public virtual bool IsVisible { get; set; } = true;

    /// <summary>
    /// الطلب المرتبط (معطل)
    /// </summary>
    // public virtual Order? Order { get; set; }

    /// <summary>
    /// العميل المرتبط
    /// </summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// المتجر المرتبط (النشاط التجاري)
    /// </summary>
    public virtual Store? Store { get; set; }

    /// <summary>
    /// المندوب المرتبط (معطل)
    /// </summary>
    // public virtual Courier? Courier { get; set; }

    protected Review()
    {
    }

    public Review(
        Guid id,
        Guid customerId,
        Guid storeId,
        int storeRating,
        string? comment = null,
        int? productQualityRating = null)
        : base(id)
    {
        CustomerId = customerId;
        StoreId = storeId;
        StoreRating = storeRating;
        Comment = comment;
        ProductQualityRating = productQualityRating;
        IsVisible = true;
    }
}

/// <summary>
/// كيان قائمة المفضلة للعميل (للمتاجر أو المنتجات)
/// </summary>
public class Favorite : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// معرف العميل صاحب المفضلة
    /// </summary>
    public virtual Guid CustomerId { get; set; }

    /// <summary>
    /// نوع الكيان المفضل (Store, Product)
    /// </summary>
    public virtual string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// معرف الكيان المفضل (معرف المتجر أو معرف المنتج)
    /// </summary>
    public virtual Guid EntityId { get; set; }

    /// <summary>
    /// العميل المرتبط
    /// </summary>
    public virtual Customer? Customer { get; set; }

    protected Favorite()
    {
    }

    public Favorite(Guid id, Guid customerId, string entityType, Guid entityId)
        : base(id)
    {
        CustomerId = customerId;
        EntityType = entityType;
        EntityId = entityId;
    }
}
