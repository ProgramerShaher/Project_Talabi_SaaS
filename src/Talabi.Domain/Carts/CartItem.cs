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
        string? notes = null)
        : base(id)
    {
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
        UnitPriceAtAddition = unitPriceAtAddition;
        TotalPrice = unitPriceAtAddition * quantity;
        Notes = notes;
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
