namespace Talabi.Auth.Dtos;

/// <summary>
/// نموذج بيانات تأكيد البريد الإلكتروني برمز التحقق (OTP)
/// </summary>
public class VerifyEmailDto
{
    /// <summary>
    /// البريد الإلكتروني الذي تم التسجيل به
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// كود التحقق المرسل إلى البريد (6 أرقام)
    /// </summary>
    public string Code { get; set; } = string.Empty;
}
