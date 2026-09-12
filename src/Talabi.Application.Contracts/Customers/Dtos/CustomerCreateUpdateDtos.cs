using System;
using System.ComponentModel.DataAnnotations;

namespace Talabi.Customers.Dtos;

/// <summary>
/// كائن إنشاء عميل جديد
/// </summary>
public class CreateCustomerDto
{
    #region Properties
    /// <summary>
    /// معرف حساب المستخدم الأساسي
    /// </summary>
    [Required(ErrorMessage = "معرف المستخدم مطلوب")]
    public Guid UserId { get; set; }

    /// <summary>
    /// تاريخ ميلاد العميل
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// جنس العميل
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// اللغة المفضلة للواجهة
    /// </summary>
    [StringLength(CustomerConsts.MaxPreferredLanguageLength, ErrorMessage = "تجاوزت الحد الأقصى لطول رمز اللغة")]
    public string PreferredLanguage { get; set; } = CustomerConsts.DefaultPreferredLanguage;
    #endregion
}

/// <summary>
/// كائن تعديل بيانات العميل
/// </summary>
public class UpdateCustomerDto
{
    #region Properties
    /// <summary>
    /// تاريخ ميلاد العميل
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// جنس العميل
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// اللغة المفضلة للواجهة
    /// </summary>
    [StringLength(CustomerConsts.MaxPreferredLanguageLength, ErrorMessage = "تجاوزت الحد الأقصى لطول رمز اللغة")]
    public string PreferredLanguage { get; set; } = CustomerConsts.DefaultPreferredLanguage;
    #endregion
}
