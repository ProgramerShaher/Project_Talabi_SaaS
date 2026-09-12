using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Stores;

/// <summary>
/// كيان موظفي ومسؤولي المتاجر وربطهم بالمستخدمين
/// </summary>
public class StoreUser : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// معرف المتجر التابع له الموظف
    /// </summary>
    public virtual Guid StoreId { get; set; }

    /// <summary>
    /// معرف المستخدم في جدول المستخدمين الأساسي
    /// </summary>
    public virtual Guid UserId { get; set; }

    /// <summary>
    /// دور وصلاحية الموظف في المتجر (مالك، مدير، موظف)
    /// </summary>
    public virtual StoreUserRole Role { get; set; }

    /// <summary>
    /// تاريخ ووقت الانضمام للمتجر
    /// </summary>
    public virtual DateTime JoinedAt { get; set; }

    /// <summary>
    /// المتجر المرتبط
    /// </summary>
    public virtual Store? Store { get; set; }

    protected StoreUser()
    {
    }

    public StoreUser(Guid id, Guid storeId, Guid userId, StoreUserRole role, DateTime joinedAt)
        : base(id)
    {
        StoreId = storeId;
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }
}
