using System;
using FluentValidation;
using Talabi.Customers;
using Talabi.Customers.Dtos;

namespace Talabi.Customers.Validators;

#region Customer Validators
/// <summary>
/// محدد قواعد التحقق الصارمة لإنشاء عميل جديد
/// </summary>
public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerDtoValidator()
    {
        #region Rules
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("معرف المستخدم (UserId) إلزامي ولا يمكن أن يكون فارغاً.");

        When(x => x.DateOfBirth.HasValue, () =>
        {
            RuleFor(x => x.DateOfBirth!.Value)
                .LessThan(DateTime.UtcNow.AddDays(1))
                .WithMessage("تاريخ الميلاد يجب أن يكون في الماضي.")
                .GreaterThan(DateTime.UtcNow.AddYears(-120))
                .WithMessage("تاريخ الميلاد غير منطقي (يجب ألا يتجاوز 120 عاماً).");
        });

        When(x => x.Gender.HasValue, () =>
        {
            RuleFor(x => x.Gender!.Value)
                .IsInEnum()
                .WithMessage("قيمة الجنس المحددة غير صالحة ضمن خيارات النظام.");
        });

        RuleFor(x => x.PreferredLanguage)
            .NotEmpty()
            .WithMessage("رمز اللغة المفضلة مطلوب.")
            .MaximumLength(CustomerConsts.MaxPreferredLanguageLength)
            .WithMessage($"رمز اللغة المفضلة لا يمكن أن يتجاوز {CustomerConsts.MaxPreferredLanguageLength} أحرف.")
            .Matches(@"^[a-zA-Z]{2,5}(-[a-zA-Z0-9]{2,5})*$")
            .WithMessage("صيغة رمز اللغة المفضلة غير صالحة (مثال: 'ar' أو 'en' أو 'ar-YE').");
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لتعديل بيانات العميل
/// </summary>
public class UpdateCustomerDtoValidator : AbstractValidator<UpdateCustomerDto>
{
    public UpdateCustomerDtoValidator()
    {
        #region Rules
        When(x => x.DateOfBirth.HasValue, () =>
        {
            RuleFor(x => x.DateOfBirth!.Value)
                .LessThan(DateTime.UtcNow.AddDays(1))
                .WithMessage("تاريخ الميلاد يجب أن يكون في الماضي.")
                .GreaterThan(DateTime.UtcNow.AddYears(-120))
                .WithMessage("تاريخ الميلاد غير منطقي (يجب ألا يتجاوز 120 عاماً).");
        });

        When(x => x.Gender.HasValue, () =>
        {
            RuleFor(x => x.Gender!.Value)
                .IsInEnum()
                .WithMessage("قيمة الجنس المحددة غير صالحة ضمن خيارات النظام.");
        });

        RuleFor(x => x.PreferredLanguage)
            .NotEmpty()
            .WithMessage("رمز اللغة المفضلة مطلوب.")
            .MaximumLength(CustomerConsts.MaxPreferredLanguageLength)
            .WithMessage($"رمز اللغة المفضلة لا يمكن أن يتجاوز {CustomerConsts.MaxPreferredLanguageLength} أحرف.")
            .Matches(@"^[a-zA-Z]{2,5}(-[a-zA-Z0-9]{2,5})*$")
            .WithMessage("صيغة رمز اللغة المفضلة غير صالحة (مثال: 'ar' أو 'en' أو 'ar-YE').");
        #endregion
    }
}
#endregion

#region Customer Address Validators
/// <summary>
/// محدد قواعد التحقق الصارمة لعناوين العملاء
/// </summary>
public class CreateUpdateCustomerAddressDtoValidator : AbstractValidator<CreateUpdateCustomerAddressDto>
{
    public CreateUpdateCustomerAddressDtoValidator()
    {
        #region Rules
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("تسمية العنوان مطلوبة (مثل: المنزل، العمل).")
            .MaximumLength(CustomerAddressConsts.MaxTitleLength)
            .WithMessage($"تسمية العنوان لا يمكن أن تتجاوز {CustomerAddressConsts.MaxTitleLength} حرفاً.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0m, 90.0m)
            .WithMessage("إحداثيات خط العرض (Latitude) يجب أن تكون محصورة بين -90 و 90 درجة.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0m, 180.0m)
            .WithMessage("إحداثيات خط الطول (Longitude) يجب أن تكون محصورة بين -180 و 180 درجة.");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("اسم المدينة مطلوب.")
            .MaximumLength(CustomerAddressConsts.MaxCityLength)
            .WithMessage($"اسم المدينة لا يمكن أن يتجاوز {CustomerAddressConsts.MaxCityLength} حرفاً.");

        RuleFor(x => x.District)
            .NotEmpty()
            .WithMessage("اسم الحي أو المنطقة مطلوب.")
            .MaximumLength(CustomerAddressConsts.MaxDistrictLength)
            .WithMessage($"اسم الحي لا يمكن أن يتجاوز {CustomerAddressConsts.MaxDistrictLength} حرفاً.");

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("اسم الشارع مطلوب.")
            .MaximumLength(CustomerAddressConsts.MaxStreetLength)
            .WithMessage($"اسم الشارع لا يمكن أن يتجاوز {CustomerAddressConsts.MaxStreetLength} حرفاً.");

        When(x => !string.IsNullOrEmpty(x.Building), () =>
        {
            RuleFor(x => x.Building)
                .MaximumLength(CustomerAddressConsts.MaxBuildingLength)
                .WithMessage($"رقم أو اسم المبنى لا يمكن أن يتجاوز {CustomerAddressConsts.MaxBuildingLength} حرفاً.");
        });

        When(x => !string.IsNullOrEmpty(x.Floor), () =>
        {
            RuleFor(x => x.Floor)
                .MaximumLength(CustomerAddressConsts.MaxFloorLength)
                .WithMessage($"رقم الطابق لا يمكن أن يتجاوز {CustomerAddressConsts.MaxFloorLength} حرفاً.");
        });

        When(x => !string.IsNullOrEmpty(x.Apartment), () =>
        {
            RuleFor(x => x.Apartment)
                .MaximumLength(CustomerAddressConsts.MaxApartmentLength)
                .WithMessage($"رقم الشقة لا يمكن أن يتجاوز {CustomerAddressConsts.MaxApartmentLength} حرفاً.");
        });

        When(x => !string.IsNullOrEmpty(x.AdditionalDetails), () =>
        {
            RuleFor(x => x.AdditionalDetails)
                .MaximumLength(CustomerAddressConsts.MaxAdditionalDetailsLength)
                .WithMessage($"تفاصيل العنوان الإضافية لا يمكن أن تتجاوز {CustomerAddressConsts.MaxAdditionalDetailsLength} حرفاً.");
        });
        #endregion
    }
}
#endregion
