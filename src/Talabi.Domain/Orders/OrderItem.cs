using System;
using Talabi.Products;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Orders;

/// <summary>
/// كيان عنصر أو منتج داخل الطلب (مع لقطة للبيانات وقت الطلب لمنع التغير)
/// </summary>
public class OrderItem : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// معرف الطلب التابع له
    /// </summary>
    public virtual Guid OrderId { get; set; }

    /// <summary>
    /// معرف المنتج الأصلي في النظام
    /// </summary>
    public virtual Guid ProductId { get; set; }

    /// <summary>
    /// اسم المنتج وقت إنشاء الطلب (Snapshot)
    /// </summary>
    public virtual string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// رابط صورة المنتج وقت إنشاء الطلب (Snapshot)
    /// </summary>
    public virtual string? ProductImageUrl { get; set; }

    /// <summary>
    /// كود المنتج SKU وقت الطلب (Snapshot)
    /// </summary>
    public virtual string SKU { get; set; } = string.Empty;

    /// <summary>
    /// وحدة القياس وقت الطلب (Snapshot)
    /// </summary>
    public virtual string Unit { get; set; } = string.Empty;

    /// <summary>
    /// معرف وحدة البيع المحددة (اختياري)
    /// </summary>
    public virtual Guid? SalesUnitId { get; set; }

    /// <summary>
    /// الكمية المطلوبة
    /// </summary>
    public virtual int Quantity { get; set; }

    /// <summary>
    /// سعر الوحدة الفعلي وقت الطلب
    /// </summary>
    public virtual decimal UnitPrice { get; set; }

    /// <summary>
    /// قيمة الخصم المالي المطبق على السلعة
    /// </summary>
    public virtual decimal Discount { get; set; }

    /// <summary>
    /// السعر الإجمالي للكمية ( (UnitPrice * Quantity) - Discount )
    /// </summary>
    public virtual decimal TotalPrice { get; set; }

    /// <summary>
    /// ملاحظات العميل الخاصة بهذا الصنف
    /// </summary>
    public virtual string? Notes { get; set; }

    /// <summary>
    /// الطلب المرتبط
    /// </summary>
    public virtual Order? Order { get; set; }

    /// <summary>
    /// المنتج المرتبط
    /// </summary>
    public virtual Product? Product { get; set; }

    /// <summary>
    /// وحدة البيع المرتبطة إن وجدت
    /// </summary>
    public virtual SalesUnit? SalesUnit { get; set; }

    protected OrderItem()
    {
    }

    public OrderItem(
        Guid id,
        Guid orderId,
        Guid productId,
        string productName,
        string sku,
        string unit,
        int quantity,
        decimal unitPrice,
        decimal discount = 0,
        string? notes = null,
        string? productImageUrl = null,
        Guid? salesUnitId = null)
        : base(id)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        SKU = sku;
        Unit = unit;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = discount;
        TotalPrice = Math.Max(0, (unitPrice * quantity) - discount);
        Notes = notes;
        ProductImageUrl = productImageUrl;
        SalesUnitId = salesUnitId;
    }
}
