using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Talabi.Products;

/// <summary>
/// كيان وحدة البيع المستقلة في النظام (Sales Unit Aggregate Root)
/// مثل: حبة / وحدة، كيلو، كيس، لتر، كرتون
/// </summary>
public class SalesUnit : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    #region Properties

    /// <summary>
    /// معرف المستأجر في نظام SaaS (اختياري، لدعم الوحدات العامة أو المخصصة)
    /// </summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>
    /// اسم وحدة البيع (مثال: كيلو، كيس، حبة، لتر)
    /// </summary>
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// رمز اختصار الوحدة (مثال: KG, Bag, PCS, Ltr)
    /// </summary>
    public virtual string? Code { get; set; }

    /// <summary>
    /// وصف توضيحي لوحدة القياس أو البيع
    /// </summary>
    public virtual string? Description { get; set; }

    /// <summary>
    /// هل وحدة البيع نشطة ومتاحة للاستخدام مع المنتجات؟
    /// </summary>
    public virtual bool IsActive { get; set; } = true;

    /// <summary>
    /// ترتيب الظهور في القوائم المنسدلة
    /// </summary>
    public virtual int DisplayOrder { get; set; }

    #endregion

    #region Constructors

    protected SalesUnit()
    {
    }

    public SalesUnit(
        Guid id,
        string name,
        string? code = null,
        string? description = null,
        int displayOrder = 0,
        bool isActive = true,
        Guid? tenantId = null)
        : base(id)
    {
        Name = name;
        Code = code;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        TenantId = tenantId;
    }

    #endregion
}
