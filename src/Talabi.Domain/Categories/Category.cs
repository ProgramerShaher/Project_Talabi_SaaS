using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Categories;

/// <summary>
/// كيان التصنيف العام للمنتجات (Hierarchical Global Category)
/// </summary>
public class Category : FullAuditedAggregateRoot<Guid>
{
    /// <summary>
    /// معرف التصنيف الأب (للهيكلية الشجرية)
    /// </summary>
    public virtual Guid? ParentId { get; set; }

    /// <summary>
    /// اسم التصنيف
    /// </summary>
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// الرابط اللطيف للتصنيف (Slug)
    /// </summary>
    public virtual string Slug { get; set; } = string.Empty;

    /// <summary>
    /// وصف التصنيف
    /// </summary>
    public virtual string? Description { get; set; }

    /// <summary>
    /// رابط الأيقونة
    /// </summary>
    public virtual string? IconUrl { get; set; }

    /// <summary>
    /// رابط الصورة التوضيحية
    /// </summary>
    public virtual string? ImageUrl { get; set; }

    /// <summary>
    /// هل هو تصنيف عام معتمد على مستوى النظام؟
    /// </summary>
    public virtual bool IsGlobal { get; set; } = true;

    /// <summary>
    /// هل التصنيف نشط ومفعل؟
    /// </summary>
    public virtual bool IsActive { get; set; } = true;

    /// <summary>
    /// ترتيب عرض التصنيف
    /// </summary>
    public virtual int SortOrder { get; set; }

    /// <summary>
    /// مستوى العمق في الشجرة (0 للأصل، 1 للفرع...)
    /// </summary>
    public virtual int Level { get; set; }

    /// <summary>
    /// المسار الهرمي للتصنيف (مثل: /1/5/12/) لتسريع الاستعلام
    /// </summary>
    public virtual string? Path { get; set; }

    /// <summary>
    /// التصنيف الأب المرتبط
    /// </summary>
    public virtual Category? Parent { get; set; }

    /// <summary>
    /// قائمة التصنيفات الفرعية
    /// </summary>
    public virtual ICollection<Category> Children { get; protected set; } = new List<Category>();

    protected Category()
    {
    }

    public Category(
        Guid id,
        string name,
        string slug,
        Guid? parentId = null,
        int sortOrder = 0,
        int level = 0,
        bool isGlobal = true,
        bool isActive = true)
        : base(id)
    {
        Name = name;
        Slug = slug;
        ParentId = parentId;
        SortOrder = sortOrder;
        Level = level;
        IsGlobal = isGlobal;
        IsActive = isActive;
    }
}
