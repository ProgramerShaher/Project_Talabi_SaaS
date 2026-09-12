using System;
using System.Collections.Generic;

namespace Talabi.Auth.Dtos;

/// <summary>
/// بيانات الملف الشخصي للمستخدم لتطبيق الموبايل
/// </summary>
public class MobileProfileDto
{
    /// <summary>
    /// معرف المستخدم الأساسي (IdentityUser)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// اسم المستخدم
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// لقب المستخدم
    /// </summary>
    public string? Surname { get; set; }

    /// <summary>
    /// البريد الإلكتروني للمستخدم
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// رقم هاتف المستخدم
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// قائمة بالأدوار والصلاحيات الممنوحة للمستخدم
    /// </summary>
    public List<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// معرف العميل في حال كان المستخدم مسجلاً كعميل (Customer)
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// معرف المتجر في حال كان المستخدم يدير أو يتبع لمتجر (StoreUser)
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// معرف مندوب التوصيل في حال كان المستخدم مسجلاً كمندوب (Courier)
    /// </summary>
    public Guid? CourierId { get; set; }
}
