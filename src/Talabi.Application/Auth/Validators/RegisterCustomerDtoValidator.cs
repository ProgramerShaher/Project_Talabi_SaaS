using FluentValidation;
using Talabi.Auth.Dtos;

namespace Talabi.Auth.Validators;

/// <summary>
/// فئة التحقق من صحة بيانات التسجيل
/// </summary>
public class RegisterCustomerDtoValidator : AbstractValidator<RegisterCustomerDto>
{
    public RegisterCustomerDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("الاسم الأول مطلوب")
            .MaximumLength(50).WithMessage("الاسم يجب أن لا يتجاوز 50 حرفاً");

        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("اللقب أو اسم العائلة مطلوب")
            .MaximumLength(50).WithMessage("اللقب يجب أن لا يتجاوز 50 حرفاً");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة")
            .MinimumLength(6).WithMessage("كلمة المرور يجب أن لا تقل عن 6 أحرف");

        // التحقق من رقم الهاتف لشركات الاتصالات اليمنية
        // يبدأ بـ 77 أو 78 أو 73 أو 71 أو 70 ومكون من 9 أرقام بالضبط
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("رقم الهاتف مطلوب")
            .Matches(@"^(77|78|73|71|70)\d{7}$")
            .WithMessage("رقم الهاتف غير صحيح. يجب أن يكون 9 أرقام ويبدأ بـ 77، 78، 73، 71، أو 70");
    }
}
