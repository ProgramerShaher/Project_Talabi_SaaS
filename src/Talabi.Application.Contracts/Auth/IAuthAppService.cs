using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Talabi.Auth.Dtos;

namespace Talabi.Auth;

/// <summary>
/// واجهة خدمة المصادقة والملف الشخصي لتطبيق الموبايل
/// </summary>
public interface IAuthAppService : IApplicationService
{
    /// <summary>
    /// جلب بيانات الملف الشخصي للمستخدم الحالي بعد تسجيل الدخول (بعد الحصول على التوكن)
    /// </summary>
    /// <returns>بيانات المستخدم المخصصة لتطبيق الموبايل</returns>
    Task<MobileProfileDto> GetMobileProfileAsync();

    /// <summary>
    /// تسجيل حساب عميل جديد (الخطوة الأولى) وإرسال كود التحقق
    /// </summary>
    Task RegisterAsync(RegisterCustomerDto input);

    /// <summary>
    /// التحقق من البريد الإلكتروني باستخدام الكود المرسل
    /// </summary>
    Task VerifyEmailAsync(VerifyEmailDto input);

    /// <summary>
    /// استكمال بيانات الملف الشخصي للعميل (الخطوة الثانية) بعد تسجيل الدخول
    /// </summary>
    Task CompleteProfileAsync(CompleteCustomerProfileDto input);
}
