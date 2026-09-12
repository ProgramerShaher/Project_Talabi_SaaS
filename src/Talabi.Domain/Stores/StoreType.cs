using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Stores;

/// <summary>
/// كيان نوع أو نشاط المتجر (مثل: سوبرماركت، مطعم، صيدلية، إلكترونيات)
/// </summary>
public class StoreType : FullAuditedEntity<Guid>
{
    /// <summary>
    /// اسم نوع المتجر
    /// </summary>
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// رابط الأيقونة أو الصورة الرمزية
    /// </summary>
    public virtual string? IconUrl { get; set; }

    /// <summary>
    /// ترتيب العرض في الواجهات
    /// </summary>
    public virtual int DisplayOrder { get; set; }

    /// <summary>
    /// هل النوع مفعل ونشط؟
    /// </summary>
    public virtual bool IsActive { get; set; }

    /// <summary>
    /// قائمة المتاجر التابعة لهذا النوع
    /// </summary>
    public virtual ICollection<Store> Stores { get; protected set; } = new List<Store>();

    protected StoreType()
    {
    }

    public StoreType(Guid id, string name, int displayOrder = 0, bool isActive = true)
        : base(id)
    {
        Name = name;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }
}
