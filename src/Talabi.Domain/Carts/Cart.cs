using System;
using System.Collections.Generic;
using System.Linq;
using Talabi.Customers;
using Talabi.Stores;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Carts;

/// <summary>
/// كيان سلة المشتريات للعميل
/// </summary>
public class Cart : FullAuditedAggregateRoot<Guid>
{
    #region 1. Properties

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
    /// إجمالي أسعار المنتجات قبل الخصم وفق آخر إعادة حساب للسلة
    /// </summary>
    public virtual decimal SubTotal { get; protected set; }

    /// <summary>
    /// إجمالي الخصومات وفق آخر إعادة حساب للسلة
    /// </summary>
    public virtual decimal TotalDiscount { get; protected set; }

    /// <summary>
    /// الإجمالي النهائي وفق آخر إعادة حساب للسلة
    /// </summary>
    public virtual decimal FinalTotal { get; protected set; }

    #endregion

    #region 2. Navigation Properties & Relations

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

    #endregion

    #region 3. Constructors

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

    #endregion

    #region 4. Business Logic Methods

    public virtual CartItem AddItem(
        Guid itemId,
        Guid productId,
        int quantity,
        decimal unitPrice,
        string? notes = null,
        Guid? salesUnitId = null,
        string? unitName = null)
    {
        var existingItem = Items.FirstOrDefault(x => x.ProductId == productId && x.SalesUnitId == salesUnitId);
        if (existingItem is not null)
        {
            existingItem.ChangeQuantity(existingItem.Quantity + quantity, unitPrice);
            Touch();
            return existingItem;
        }

        var item = new CartItem(itemId, Id, productId, quantity, unitPrice, notes, salesUnitId, unitName);
        Items.Add(item);
        Touch();
        return item;
    }

    public virtual void ChangeStore(Guid storeId)
    {
        if (Items.Count != 0 && StoreId != storeId)
        {
            return;
        }

        StoreId = storeId;
        Touch();
    }

    public virtual void UpdateItemQuantity(Guid itemId, int quantity, decimal unitPrice)
    {
        var item = Items.First(x => x.Id == itemId);
        item.ChangeQuantity(quantity, unitPrice);
        Touch();
    }

    public virtual void RemoveItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(x => x.Id == itemId);
        if (item is null)
        {
            return;
        }

        Items.Remove(item);
        Touch();
    }

    public virtual void Clear()
    {
        Items.Clear();
        SubTotal = 0m;
        TotalDiscount = 0m;
        FinalTotal = 0m;
        Touch();
    }

    public virtual void RecalculateTotals(IEnumerable<CartItemPriceSnapshot> prices)
    {
        var priceLookup = prices.ToDictionary(x => (x.ProductId, x.SalesUnitId));

        SubTotal = 0m;
        TotalDiscount = 0m;
        FinalTotal = 0m;

        foreach (var item in Items)
        {
            if (!priceLookup.TryGetValue((item.ProductId, item.SalesUnitId), out var price))
            {
                continue;
            }

            SubTotal += price.OriginalUnitPrice * item.Quantity;
            TotalDiscount += price.UnitDiscount * item.Quantity;
            FinalTotal += price.CurrentUnitPrice * item.Quantity;
        }

        Touch();
    }

    private void Touch()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    #endregion
}

public sealed record CartItemPriceSnapshot(
    Guid ProductId,
    Guid? SalesUnitId,
    decimal OriginalUnitPrice,
    decimal CurrentUnitPrice)
{
    public decimal UnitDiscount => Math.Max(0m, OriginalUnitPrice - CurrentUnitPrice);
}
