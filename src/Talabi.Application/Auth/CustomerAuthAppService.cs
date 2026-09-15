using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Talabi.Auth.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Talabi.Auth;

/// <summary>
/// خدمة التطبيق لإدارة أمان حساب العميل
/// </summary>
public class CustomerAuthAppService : ApplicationService, ICustomerAuthAppService
{
    private readonly IdentityUserManager _userManager;

    public CustomerAuthAppService(IdentityUserManager userManager)
    {
        _userManager = userManager;
    }

    [Authorize]
    public async Task ChangePasswordAsync(ChangePasswordDto input)
    {
        var user = await _userManager.GetByIdAsync(CurrentUser.GetId());
        
        var result = await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
        
        if (!result.Succeeded)
        {
            throw new UserFriendlyException("فشل تغيير كلمة المرور. يرجى التأكد من أن كلمة المرور الحالية صحيحة وأن كلمة المرور الجديدة تطابق المعايير الأمنية.");
        }
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto input)
    {
        var user = await _userManager.FindByEmailAsync(input.Email);
        
        if (user == null)
        {
            // لا نرمي خطأ إذا لم يوجد الإيميل لأسباب أمنية (حتى لا يعرف المخترق من المسجل ومن لا)
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // TODO: هنا يتم إرسال الإيميل الفعلي للعميل عبر IEmailSender
        // مؤقتاً في بيئة التطوير، سنقوم بطباعة الكود في الـ Log ليتمكن المطور من اختباره
        Logger.LogInformation("=========================================");
        Logger.LogInformation($"Password Reset Token for {input.Email}:");
        Logger.LogInformation(token);
        Logger.LogInformation("=========================================");
    }

    public async Task ResetPasswordAsync(ResetPasswordDto input)
    {
        var user = await _userManager.FindByEmailAsync(input.Email);
        
        if (user == null)
        {
            throw new UserFriendlyException("البريد الإلكتروني أو رمز التحقق غير صحيح");
        }

        var result = await _userManager.ResetPasswordAsync(user, input.ResetToken, input.NewPassword);
        
        if (!result.Succeeded)
        {
            throw new UserFriendlyException("فشل إعادة تعيين كلمة المرور. قد يكون رمز التحقق منتهي الصلاحية أو غير صحيح.");
        }
    }
}
