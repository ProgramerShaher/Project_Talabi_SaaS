using System;
using System.Collections.Generic;
using Talabi.Stores;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Categories;

/// <summary>
/// كيان تصنيفات المتاجر المخصصة (Store Category Hierarchy)
/// </summary>
public class StoreCategory : FullAuditedEntity<Guid>
{
    /// <summary>
    /// معرف المتجر التابع له التصنيف
    /// </summary>
    public virtual Guid StoreId { get; set; }

    /// <summary>
    /// معرف التصنيف الأب داخل المتجر (للهيكلية الشجرية)
    /// </summary>
    public virtual Guid? ParentId { get; set; }

    /// <summary>
    /// معرف التصنيف العام المرتبط به (اختياري للربط مع التصنيف العام للنظام)
    /// </summary>
    public virtual Guid? GlobalCategoryId { get; set; }

    /// <summary>
    /// اسم مخصص للتصنيف في هذا المتجر
    /// </summary>
    public virtual string? CustomName { get; set; }

    /// <summary>
    /// أيقونة مخصصة للتصنيف في هذا المتجر
    /// </summary>
    public virtual string? CustomIconUrl { get; set; }

    /// <summary>
    /// وصف التصنيف الخاص بالمتجر
    /// </summary>
    public virtual string? Description { get; set; }

    /// <summary>
    /// هل التصنيف مخفي عن العملاء في واجهة المتجر؟
    /// </summary>
    public virtual bool IsHidden { get; set; }

    /// <summary>
    /// هل التصنيف مفعل؟
    /// </summary>
    public virtual bool IsActive { get; set; } = true;

    /// <summary>
    /// ترتيب العرض داخل المتجر
    /// </summary>
    public virtual int SortOrder { get; set; }

    /// <summary>
    /// المتجر المرتبط
    /// </summary>
    public virtual Store? Store { get; set; }

    /// <summary>
    /// التصنيف العام المرتبط
    /// </summary>
    public virtual Category? GlobalCategory { get; set; }

    /// <summary>
    /// التصنيف الأب داخل نفس المتجر
    /// </summary>
    public virtual StoreCategory? Parent { get; set; }

    /// <summary>
    /// قائمة التصنيفات الفرعية للمتجر
    /// </summary>
    public virtual ICollection<StoreCategory> Children { get; protected set; } = new List<StoreCategory>();

    protected StoreCategory()
    {
    }

    public StoreCategory(
        Guid id,
        Guid storeId,
        string? customName,
        Guid? parentId = null,
        Guid? globalCategoryId = null,
        int sortOrder = 0)
        : base(id)
    {
        StoreId = storeId;
        CustomName = customName;
        ParentId = parentId;
        GlobalCategoryId = globalCategoryId;
        SortOrder = sortOrder;
        IsActive = true;
        IsHidden = false;
    }
}
