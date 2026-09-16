using System;
using Talabi.Products;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Carts;

/// <summary>
/// كيان عنصر أو منتج داخل سلة المشتريات
/// </summary>
public class CartItem : CreationAuditedEntity<Guid>
{
    #region 1. Properties

    /// <summary>
    /// معرف السلة التابع لها العنصر
    /// </summary>
    public virtual Guid CartId { get; set; }

    /// <summary>
    /// معرف المنتج
    /// </summary>
    public virtual Guid ProductId { get; set; }

    /// <summary>
    /// الكمية المطلوبة
    /// </summary>
    public virtual int Quantity { get; set; }

    /// <summary>
    /// معرف وحدة البيع المحددة (اختياري - null إذا كان المنتج يباع بالحبة/بدون وحدات مخصصة)
    /// </summary>
    public virtual Guid? SalesUnitId { get; set; }

    /// <summary>
    /// اسم وحدة البيع وقت الإضافة (مثل: كيس، كيلو، حبة)
    /// </summary>
    public virtual string UnitName { get; set; } = "حبة";

    /// <summary>
    /// سعر الوحدة وقت إضافتها للسلة
    /// </summary>
    public virtual decimal UnitPriceAtAddition { get; set; }

    /// <summary>
    /// إجمالي سعر السطر وفق آخر سعر محفوظ في السلة
    /// </summary>
    public virtual decimal TotalPrice { get; protected set; }

    /// <summary>
    /// ملاحظات العميل الخاصة بهذا المنتج
    /// </summary>
    public virtual string? Notes { get; set; }

    /// <summary>
    /// وقت وتاريخ الإضافة
    /// </summary>
    public virtual DateTime AddedAt { get; set; }

    #endregion

    #region 2. Navigation Properties & Relations

    /// <summary>
    /// السلة المرتبطة
    /// </summary>
    public virtual Cart? Cart { get; set; }

    /// <summary>
    /// المنتج المرتبط
    /// </summary>
    public virtual Product? Product { get; set; }

    /// <summary>
    /// وحدة البيع المرتبطة إن وجدت
    /// </summary>
    public virtual SalesUnit? SalesUnit { get; set; }

    #endregion

    #region 3. Constructors

    protected CartItem()
    {
    }

    public CartItem(
        Guid id,
        Guid cartId,
        Guid productId,
        int quantity,
        decimal unitPriceAtAddition,
        string? notes = null,
        Guid? salesUnitId = null,
        string? unitName = null)
        : base(id)
    {
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
        UnitPriceAtAddition = unitPriceAtAddition;
        TotalPrice = unitPriceAtAddition * quantity;
        Notes = notes;
        SalesUnitId = salesUnitId;
        UnitName = string.IsNullOrWhiteSpace(unitName) ? "حبة" : unitName;
        AddedAt = DateTime.UtcNow;
    }

    #endregion

    #region 4. Business Logic Methods

    public virtual void ChangeQuantity(int quantity, decimal unitPrice)
    {
        Quantity = quantity;
        UnitPriceAtAddition = unitPrice;
        TotalPrice = unitPrice * quantity;
    }

    #endregion
}
