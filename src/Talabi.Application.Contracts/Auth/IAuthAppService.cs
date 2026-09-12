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
}
