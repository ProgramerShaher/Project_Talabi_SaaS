using System.Threading.Tasks;
using Talabi.Auth.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Auth;

/// <summary>
/// واجهة خدمة أمان حساب العميل والمصادقة
/// </summary>
public interface ICustomerAuthAppService : IApplicationService
{
    /// <summary>
    /// تغيير كلمة المرور للعميل المسجل دخوله
    /// </summary>
    Task ChangePasswordAsync(ChangePasswordDto input);

    /// <summary>
    /// طلب استعادة كلمة المرور عبر الإيميل
    /// </summary>
    Task ForgotPasswordAsync(ForgotPasswordDto input);

    /// <summary>
    /// إعادة تعيين كلمة المرور باستخدام الكود المستلم
    /// </summary>
    Task ResetPasswordAsync(ResetPasswordDto input);
}
