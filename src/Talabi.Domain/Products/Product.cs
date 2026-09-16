using System;
using System.Collections.Generic;
using Talabi.Categories;
using Talabi.Stores;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Talabi.Products;

/// <summary>
/// كيان المنتج في المتجر (Product Aggregate Root)
/// </summary>
public class Product : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// معرف المستأجر في نظام SaaS
    /// </summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>
    /// معرف المتجر التابع له المنتج
    /// </summary>
    public virtual Guid StoreId { get; set; }

    /// <summary>
    /// معرف تصنيف المتجر الذي ينتمي له المنتج (يقبل أي مستوى: رئيسي أو فرعي - اختياري)
    /// </summary>
    public virtual Guid? StoreCategoryId { get; set; }

    /// <summary>
    /// اسم المنتج
    /// </summary>
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// الوصف التفصيلي الكامل للمنتج
    /// </summary>
    public virtual string? Description { get; set; }

    /// <summary>
    /// وصف مختصر وسريع للمنتج
    /// </summary>
    public virtual string? ShortDescription { get; set; }

    /// <summary>
    /// رمز التخزين الفريد للمنتج (SKU)
    /// </summary>
    public virtual string SKU { get; set; } = string.Empty;

    // /// <summary>
    // /// الباركود الدولي أو الداخلي للمنتج - معطل بناءً على متطلبات النظام
    // /// </summary>
    // public virtual string? Barcode { get; set; }

    /// <summary>
    /// السعر الأساسي للمنتج
    /// </summary>
    public virtual decimal Price { get; set; }

    /// <summary>
    /// قيمة الخصم المالي أو النسبة
    /// </summary>
    public virtual decimal Discount { get; set; }

    /// <summary>
    /// نوع الخصم (مبلغ ثابت أو نسبة مئوية)
    /// </summary>
    public virtual DiscountType DiscountType { get; set; } = DiscountType.Fixed;

    /// <summary>
    /// السعر النهائي بعد تطبيق الخصم
    /// </summary>
    public virtual decimal FinalPrice { get; set; }

    // /// <summary>
    // /// سعر التكلفة على المتجر (سري ولا يظهر للعملاء) - معطل
    // /// </summary>
    // public virtual decimal? CostPrice { get; set; }

    /// <summary>
    /// وحدة القياس (حبة، كرتون، كجم، لتر)
    /// </summary>
    public virtual string Unit { get; set; } = string.Empty;

    // /// <summary>
    // /// العلامة التجارية / الماركة - معطل
    // /// </summary>
    // public virtual string? Brand { get; set; }

    // /// <summary>
    // /// وزن المنتج - معطل
    // /// </summary>
    // public virtual decimal? Weight { get; set; }

    // /// <summary>
    // /// وحدة قياس الوزن (كجم، جم) - معطل
    // /// </summary>
    // public virtual string? WeightUnit { get; set; }

    // /// <summary>
    // /// بلد المنشأ للمنتج - معطل
    // /// </summary>
    // public virtual string? Origin { get; set; }

    /// <summary>
    /// رابط الصورة الرئيسية المميزة
    /// </summary>
    public virtual string? MainImageUrl { get; set; }

    /// <summary>
    /// هل المنتج متوفر وجاهز للبيع الفوري؟
    /// </summary>
    public virtual bool IsAvailable { get; set; } = true;

    /// <summary>
    /// هل المنتج نشط في النظام؟
    /// </summary>
    public virtual bool IsActive { get; set; } = true;

    /// <summary>
    /// هل المنتج مميز في واجهة المتجر؟
    /// </summary>
    public virtual bool IsFeatured { get; set; }

    /// <summary>
    /// وسوم وكلمات مفتاحية لتسهيل البحث (مفصولة بفواصل)
    /// </summary>
    public virtual string? Tags { get; set; }

    /// <summary>
    /// الحد الأدنى للكمية المسموح بطلبها في المرة الواحدة
    /// </summary>
    public virtual int MinOrderQuantity { get; set; } = 1;

    /// <summary>
    /// الحد الأقصى للكمية المسموح بطلبها في المرة الواحدة
    /// </summary>
    public virtual int? MaxOrderQuantity { get; set; }

    /// <summary>
    /// المتجر المرتبط
    /// </summary>
    public virtual Store? Store { get; set; }

    /// <summary>
    /// تصنيف المتجر المرتبط
    /// </summary>
    public virtual StoreCategory? StoreCategory { get; set; }

    /// <summary>
    /// معرض صور المنتج الإضافية
    /// </summary>
    public virtual ICollection<ProductImage> Images { get; protected set; } = new List<ProductImage>();

    /// <summary>
    /// وحدات بيع المنتج المخصصة (اختيارية)
    /// </summary>
    public virtual ICollection<ProductSalesUnit> SalesUnits { get; protected set; } = new List<ProductSalesUnit>();

    // /// <summary>
    // /// سجل المخزون التابع لهذا المنتج - معطل لأن النظام للعرض فقط ولا يتدخل في كمية المخزون
    // /// </summary>
    // public virtual Inventory? Inventory { get; set; }

    protected Product()
    {
    }

    public Product(
        Guid id,
        Guid storeId,
        string name,
        string sku,
        decimal price,
        string unit,
        Guid? storeCategoryId = null,
        decimal discount = 0,
        DiscountType discountType = DiscountType.Fixed,
        Guid? tenantId = null)
        : base(id)
    {
        StoreId = storeId;
        StoreCategoryId = storeCategoryId;
        Name = name;
        SKU = sku;
        Price = price;
        Unit = unit;
        Discount = discount;
        DiscountType = discountType;
        TenantId = tenantId;
        IsAvailable = true;
        IsActive = true;
        MinOrderQuantity = 1;
        CalculateFinalPrice();
    }

    /// <summary>
    /// حساب السعر النهائي للمنتج بناءً على السعر ونوع الخصم
    /// </summary>
    public virtual void CalculateFinalPrice()
    {
        if (Discount <= 0)
        {
            FinalPrice = Price;
            return;
        }

        if (DiscountType == DiscountType.Percentage)
        {
            var discountValue = Price * (Discount / 100m);
            FinalPrice = Math.Max(0, Price - discountValue);
        }
        else
        {
            FinalPrice = Math.Max(0, Price - Discount);
        }
    }
}
