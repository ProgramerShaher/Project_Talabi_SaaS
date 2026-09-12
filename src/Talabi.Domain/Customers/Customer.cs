using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Customers;

/// <summary>
/// كيان العميل في النظام - يمثل الحساب والملف الشخصي للعميل
/// </summary>
public class Customer : FullAuditedAggregateRoot<Guid>
{
    /// <summary>
    /// المعرف الخارجي المرتبط بحساب المستخدم الأساسي في ABP (AbpUsers)
    /// </summary>
    public virtual Guid UserId { get; set; }

    /// <summary>
    /// تاريخ ميلاد العميل
    /// </summary>
    public virtual DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// جنس العميل (ذكر / أنثى)
    /// </summary>
    public virtual Gender? Gender { get; set; }

    /// <summary>
    /// رصيد نقاط الولاء المكتسبة
    /// </summary>
    public virtual int LoyaltyPoints { get; set; }

    /// <summary>
    /// اللغة المفضلة لواجهة العميل (ar / en)
    /// </summary>
    public virtual string PreferredLanguage { get; set; } = CustomerConsts.DefaultPreferredLanguage;

    /// <summary>
    /// قائمة عناوين التوصيل التابعة للعميل
    /// </summary>
    public virtual ICollection<CustomerAddress> Addresses { get; protected set; } = new List<CustomerAddress>();

    protected Customer()
    {
    }

    public Customer(Guid id, Guid userId, string preferredLanguage = CustomerConsts.DefaultPreferredLanguage)
        : base(id)
    {
        UserId = userId;
        PreferredLanguage = preferredLanguage;
        LoyaltyPoints = 0;
    }
}
