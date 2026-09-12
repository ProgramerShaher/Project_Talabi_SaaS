using System;
using Volo.Abp.Domain.Entities;

namespace Talabi.Orders;

/// <summary>
/// كيان حالة الطلب (جدول الحالات المعيارية للنظام)
/// </summary>
public class OrderStatus : Entity<Guid>
{
    /// <summary>
    /// الاسم البرمجي الفريد للحالة (Pending, Accepted, Processing, InTransit, Delivered...)
    /// </summary>
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// الاسم المعروض للمستخدم باللغة المفهومة
    /// </summary>
    public virtual string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// كود اللون المخصص لتمييز الحالة (Hex Code مثل #28a745)
    /// </summary>
    public virtual string? Color { get; set; }

    /// <summary>
    /// اسم أو كود الأيقونة
    /// </summary>
    public virtual string? Icon { get; set; }

    /// <summary>
    /// ترتيب تسلسل الحالة
    /// </summary>
    public virtual int DisplayOrder { get; set; }

    /// <summary>
    /// هل تمثل هذه الحالة نهاية دورة حياة الطلب؟
    /// </summary>
    public virtual bool IsFinal { get; set; }

    protected OrderStatus()
    {
    }

    public OrderStatus(
        Guid id,
        string name,
        string displayName,
        int displayOrder = 0,
        bool isFinal = false,
        string? color = null,
        string? icon = null)
        : base(id)
    {
        Name = name;
        DisplayName = displayName;
        DisplayOrder = displayOrder;
        IsFinal = isFinal;
        Color = color;
        Icon = icon;
    }
}
