using FluentValidation;
using Talabi.Auth.Dtos;

namespace Talabi.Auth.Validators;

/// <summary>
/// فئة التحقق من صحة بيانات استكمال الملف الشخصي للعميل
/// </summary>
public class CompleteCustomerProfileDtoValidator : AbstractValidator<CompleteCustomerProfileDto>
{
    public CompleteCustomerProfileDtoValidator()
    {
        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("الدولة مطلوبة")
            .MaximumLength(100).WithMessage("طول حقل الدولة يجب أن لا يتجاوز 100 حرف");

        RuleFor(x => x.Governorate)
            .NotEmpty().WithMessage("المحافظة مطلوبة")
            .MaximumLength(100).WithMessage("طول حقل المحافظة يجب أن لا يتجاوز 100 حرف");

        RuleFor(x => x.Region)
            .NotEmpty().WithMessage("المنطقة مطلوبة")
            .MaximumLength(100).WithMessage("طول حقل المنطقة يجب أن لا يتجاوز 100 حرف");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("المدينة مطلوبة")
            .MaximumLength(100).WithMessage("طول حقل المدينة يجب أن لا يتجاوز 100 حرف");

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("الحي مطلوب")
            .MaximumLength(100).WithMessage("طول حقل الحي يجب أن لا يتجاوز 100 حرف");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("الشارع مطلوب")
            .MaximumLength(200).WithMessage("طول حقل الشارع يجب أن لا يتجاوز 200 حرف");
    }
}
