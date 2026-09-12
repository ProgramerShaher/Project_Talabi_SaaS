using FluentValidation;
using Talabi.Auth.Dtos;

namespace Talabi.Auth.Validators;

/// <summary>
/// فئة التحقق من صحة بيانات تأكيد البريد الإلكتروني
/// </summary>
public class VerifyEmailDtoValidator : AbstractValidator<VerifyEmailDto>
{
    public VerifyEmailDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("كود التحقق مطلوب")
            .Length(6).WithMessage("كود التحقق يجب أن يكون 6 أرقام");
    }
}
