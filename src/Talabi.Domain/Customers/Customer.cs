using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Talabi.Customers;

/// <summary>
/// كيان العميل في النظام - يمثل الحساب والملف الشخصي للعميل
/// </summary>
public class Customer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    #region Properties
    /// <summary>
    /// معرف المستأجر
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

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
    /// رابط الصورة الشخصية للعميل
    /// </summary>
    public virtual string? AvatarUrl { get; set; }
    #endregion

    #region Navigation Properties
    /// <summary>
    /// قائمة عناوين التوصيل التابعة للعميل
    /// </summary>
    public virtual ICollection<CustomerAddress> Addresses { get; protected set; } = new List<CustomerAddress>();
    #endregion

    #region Constructors
    protected Customer()
    {
    }

    public Customer(Guid id, Guid userId, Guid? tenantId = null, string preferredLanguage = CustomerConsts.DefaultPreferredLanguage)
        : base(id)
    {
        UserId = userId;
        TenantId = tenantId;
        PreferredLanguage = preferredLanguage;
        LoyaltyPoints = 0;
    }
    #endregion
}
