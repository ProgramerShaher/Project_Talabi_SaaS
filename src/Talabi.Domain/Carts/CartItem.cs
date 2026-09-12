using System;
using Talabi.Products;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Carts;

/// <summary>
/// كيان عنصر أو منتج داخل سلة المشتريات
/// </summary>
public class CartItem : CreationAuditedEntity<Guid>
{
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
    /// ملاحظات العميل الخاصة بهذا المنتج
    /// </summary>
    public virtual string? Notes { get; set; }

    /// <summary>
    /// وقت وتاريخ الإضافة
    /// </summary>
    public virtual DateTime AddedAt { get; set; }

    /// <summary>
    /// السلة المرتبطة
    /// </summary>
    public virtual Cart? Cart { get; set; }

    /// <summary>
    /// المنتج المرتبط
    /// </summary>
    public virtual Product? Product { get; set; }

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
        Notes = notes;
        AddedAt = DateTime.UtcNow;
    }
}
