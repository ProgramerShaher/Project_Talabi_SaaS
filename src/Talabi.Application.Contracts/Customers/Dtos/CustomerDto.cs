using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Talabi.Customers.Dtos;

/// <summary>
/// كائن عرض بيانات العميل للواجهة الأمامية
/// </summary>
public class CustomerDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    /// <summary>
    /// معرف حساب المستخدم الأساسي
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// تاريخ ميلاد العميل
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// جنس العميل (ذكر / أنثى)
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// رصيد نقاط الولاء المكتسبة
    /// </summary>
    public int LoyaltyPoints { get; set; }

    /// <summary>
    /// اللغة المفضلة للعميل
    /// </summary>
    public string PreferredLanguage { get; set; } = CustomerConsts.DefaultPreferredLanguage;

    /// <summary>
    /// قائمة عناوين العميل المسجلة
    /// </summary>
    public List<CustomerAddressDto> Addresses { get; set; } = new();
    #endregion
}
