using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Talabi.Customers.Dtos;

/// <summary>
/// كائن عرض بيانات العميل الكاملة للواجهة الأمامية
/// يجمع بيانات هوية المستخدم من AbpUsers مع بيانات العميل التجارية من Customer
/// </summary>
public class CustomerDto : FullAuditedEntityDto<Guid>
{
    #region Properties

    /// <summary>
    /// معرف حساب المستخدم الأساسي في ABP (AbpUsers)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// الاسم الكامل للعميل - مجلوب من AbpUser
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// البريد الإلكتروني - مجلوب من AbpUser
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// رقم الهاتف - مجلوب من AbpUser
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// تاريخ ميلاد العميل
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// جنس العميل (ذكر / أنثى)
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// رصيد نقاط الولاء المتراكمة
    /// </summary>
    public int LoyaltyPoints { get; set; }

    /// <summary>
    /// اللغة المفضلة للعميل
    /// </summary>
    public string PreferredLanguage { get; set; } = CustomerConsts.DefaultPreferredLanguage;

    /// <summary>
    /// رابط الصورة الشخصية
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// قائمة عناوين التوصيل المسجلة للعميل
    /// </summary>
    public List<CustomerAddressDto> Addresses { get; set; } = new();

    #endregion
}