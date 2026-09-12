using System;

namespace Talabi.Auth.Dtos;

/// <summary>
/// نموذج بيانات التسجيل الأساسي للعميل الجديد
/// </summary>
public class RegisterCustomerDto
{
    /// <summary>
    /// الاسم الأول
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// اللقب أو اسم العائلة
    /// </summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// البريد الإلكتروني (يجب أن يكون صحيحاً لغرض إرسال كود التحقق)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// كلمة المرور (يجب أن تكون قوية)
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// رقم الهاتف (مثال: 77XXXXXXX)
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
}
