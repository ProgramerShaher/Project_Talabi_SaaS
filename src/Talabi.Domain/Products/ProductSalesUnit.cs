using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Products;

/// <summary>
/// كيان ربط وحدة البيع بالمنتج مع تحديد الخصائص والسعر والوحدة الافتراضية
/// </summary>
public class ProductSalesUnit : FullAuditedEntity<Guid>
{
    #region Properties

    /// <summary>
    /// معرف المنتج التابع له
    /// </summary>
    public virtual Guid ProductId { get; set; }

    /// <summary>
    /// معرف وحدة البيع الأساسية
    /// </summary>
    public virtual Guid SalesUnitId { get; set; }

    /// <summary>
    /// اسم وحدة البيع المرجعية (Snapshot لسرعة العرض دون Join)
    /// </summary>
    public virtual string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// سعر بيع المنتج بهذه الوحدة (إذا كانت فارغة أو صفر، يعتمد سعر المنتج الأساسي)
    /// </summary>
    public virtual decimal? Price { get; set; }

    /// <summary>
    /// هل هذه هي وحدة البيع الافتراضية للمنتج؟
    /// (يسمح بوحدة افتراضية واحدة فقط لكل منتج)
    /// </summary>
    public virtual bool IsDefault { get; set; }

    /// <summary>
    /// هل الوحدة مفعلة ومتاحة للشراء للعميل؟
    /// </summary>
    public virtual bool IsActive { get; set; } = true;

    /// <summary>
    /// ترتيب عرض الوحدة في قائمة خيارات الشراء
    /// </summary>
    public virtual int DisplayOrder { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// المنتج المرتبط
    /// </summary>
    public virtual Product? Product { get; set; }

    /// <summary>
    /// وحدة البيع المرتبطة
    /// </summary>
    public virtual SalesUnit? SalesUnit { get; set; }

    #endregion

    #region Constructors

    protected ProductSalesUnit()
    {
    }

    public ProductSalesUnit(
        Guid id,
        Guid productId,
        Guid salesUnitId,
        string unitName,
        decimal? price = null,
        bool isDefault = false,
        bool isActive = true,
        int displayOrder = 0)
        : base(id)
    {
        ProductId = productId;
        SalesUnitId = salesUnitId;
        UnitName = unitName;
        Price = price;
        IsDefault = isDefault;
        IsActive = isActive;
        DisplayOrder = displayOrder;
    }

    #endregion
}
