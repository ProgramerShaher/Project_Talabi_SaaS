using System;
using System.Collections.Generic;
using Talabi.Customers;
using Talabi.Stores;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Talabi.Orders;

/// <summary>
/// كيان الطلب الرئيسي (Order Aggregate Root)
/// </summary>
public class Order : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// معرف المستأجر في نظام SaaS
    /// </summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>
    /// رقم الطلب الفريد التسلسلي الظاهر للمستخدم (مثل: ORD-1001)
    /// </summary>
    public virtual string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// معرف العميل صاحب الطلب
    /// </summary>
    public virtual Guid CustomerId { get; set; }

    /// <summary>
    /// معرف المتجر المطلوب منه
    /// </summary>
    public virtual Guid StoreId { get; set; }

    /// <summary>
    /// معرف عنوان التوصيل المختار
    /// </summary>
    public virtual Guid DeliveryAddressId { get; set; }

    /// <summary>
    /// لقطة نصية كاملة لبيانات العنوان وقت إنشاء الطلب (Snapshot)
    /// </summary>
    public virtual string DeliveryAddressSnapshot { get; set; } = string.Empty;

    /// <summary>
    /// المجموع الفرعي لأسعار المنتجات
    /// </summary>
    public virtual decimal SubTotal { get; set; }

    /// <summary>
    /// إجمالي قيمة الخصم المالي
    /// </summary>
    public virtual decimal DiscountAmount { get; set; }

    // /// <summary>
    // /// رسوم التوصيل المحتسبة - معطل لأن المنصة لا تتدخل في رسوم التوصيل (المتجر مسؤول عنها)
    // /// </summary>
    // public virtual decimal DeliveryFee { get; set; }

    /// <summary>
    /// قيمة ضريبة القيمة المضافة إن وجدت
    /// </summary>
    public virtual decimal TaxAmount { get; set; }

    /// <summary>
    /// المبلغ الإجمالي النهائي المطلوب دفعه
    /// </summary>
    public virtual decimal FinalAmount { get; set; }

    /// <summary>
    /// معرف حالة الطلب الحالية
    /// </summary>
    public virtual Guid OrderStatusId { get; set; }

    /// <summary>
    /// حالة الدفع الخاصة بالطلب
    /// </summary>
    public virtual OrderPaymentStatus PaymentStatus { get; set; } = OrderPaymentStatus.Pending;

    /// <summary>
    /// معرف طريقة الدفع المختارة
    /// </summary>
    public virtual Guid PaymentMethodId { get; set; }

    /// <summary>
    /// معرف المندوب المعين لتوصيل الطلب إن وُجد
    /// </summary>
    public virtual Guid? CourierId { get; set; }

    /// <summary>
    /// ملاحظات أو تعليمات العميل على الطلب
    /// </summary>
    public virtual string? CustomerNotes { get; set; }

    /// <summary>
    /// ملاحظات داخلية من المتجر
    /// </summary>
    public virtual string? StoreNotes { get; set; }

    /// <summary>
    /// الوقت المتوقع لتسليم الطلب
    /// </summary>
    public virtual DateTime? EstimatedDeliveryTime { get; set; }

    /// <summary>
    /// الوقت الفعلي الذي تم فيه التسليم
    /// </summary>
    public virtual DateTime? ActualDeliveryTime { get; set; }

    /// <summary>
    /// وقت تأكيد وقبول الطلب من المتجر
    /// </summary>
    public virtual DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// وقت إلغاء الطلب إن تم إلغاؤه
    /// </summary>
    public virtual DateTime? CancelledAt { get; set; }

    /// <summary>
    /// العميل المرتبط
    /// </summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// المتجر المرتبط
    /// </summary>
    public virtual Store? Store { get; set; }

    /// <summary>
    /// حالة الطلب المرتبطة
    /// </summary>
    public virtual OrderStatus? OrderStatus { get; set; }

    /// <summary>
    /// تفاصيل ومنتجات الطلب (Order Items)
    /// </summary>
    public virtual ICollection<OrderItem> Items { get; protected set; } = new List<OrderItem>();

    /// <summary>
    /// سجل تتبع وتغير حالات الطلب
    /// </summary>
    public virtual ICollection<OrderStatusHistory> StatusHistories { get; protected set; } = new List<OrderStatusHistory>();

    /// <summary>
    /// سجل رفض الطلب في حال تم رفضه
    /// </summary>
    public virtual OrderRejection? Rejection { get; set; }

    /// <summary>
    /// سجل إلغاء الطلب في حال تم إلغاؤه
    /// </summary>
    public virtual OrderCancellation? Cancellation { get; set; }

    protected Order()
    {
    }

    public Order(
        Guid id,
        string orderNumber,
        Guid customerId,
        Guid storeId,
        Guid deliveryAddressId,
        string deliveryAddressSnapshot,
        Guid orderStatusId,
        Guid paymentMethodId,
        decimal subTotal,
        decimal finalAmount,
        Guid? tenantId = null)
        : base(id)
    {
        OrderNumber = orderNumber;
        CustomerId = customerId;
        StoreId = storeId;
        DeliveryAddressId = deliveryAddressId;
        DeliveryAddressSnapshot = deliveryAddressSnapshot;
        OrderStatusId = orderStatusId;
        PaymentMethodId = paymentMethodId;
        SubTotal = subTotal;
        FinalAmount = finalAmount;
        TenantId = tenantId;
        PaymentStatus = OrderPaymentStatus.Pending;
    }
}
