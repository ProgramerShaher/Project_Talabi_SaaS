using System;
using FluentValidation;
using Talabi.Stores;
using Talabi.Stores.Dtos;

namespace Talabi.Stores.Validators;

#region Store Validators
/// <summary>
/// محدد قواعد التحقق الصارمة لإنشاء متجر جديد
/// </summary>
public class CreateStoreDtoValidator : AbstractValidator<CreateStoreDto>
{
    public CreateStoreDtoValidator()
    {
        #region Rules
        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .WithMessage("معرف مالك المتجر (OwnerId) إلزامي.");

        RuleFor(x => x.StoreTypeId)
            .NotEmpty()
            .WithMessage("معرف نوع المتجر (StoreTypeId) إلزامي.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("اسم المتجر مطلوب.")
            .MaximumLength(StoreConsts.MaxNameLength)
            .WithMessage($"اسم المتجر لا يمكن أن يتجاوز {StoreConsts.MaxNameLength} حرفاً.");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("الرابط اللطيف (Slug) إلزامي.")
            .MaximumLength(StoreConsts.MaxSlugLength)
            .WithMessage($"الرابط اللطيف لا يمكن أن يتجاوز {StoreConsts.MaxSlugLength} حرفاً.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("الرابط اللطيف (Slug) يجب أن يحتوي فقط على حروف إنجليزية صغيرة، وأرقام، ومفصول بشرطة أحادية دون فراغات أو رموز خاصة.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("رقم هاتف المتجر مطلوب للتواصل.")
            .MaximumLength(StoreConsts.MaxPhoneLength)
            .WithMessage($"رقم الهاتف لا يمكن أن يتجاوز {StoreConsts.MaxPhoneLength} رقماً.")
            .Matches(@"^\+?[0-9]{7,15}$")
            .WithMessage("رقم الهاتف غير صالح، يجب أن يتكون من 7 إلى 15 رقماً مع إمكانية البدء برمز الدولة (+).");

        When(x => !string.IsNullOrEmpty(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("صيغة البريد الإلكتروني للمتجر غير صحيحة.")
                .MaximumLength(StoreConsts.MaxEmailLength)
                .WithMessage($"البريد الإلكتروني لا يمكن أن يتجاوز {StoreConsts.MaxEmailLength} حرفاً.");
        });

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("عنوان المتجر الجغرافي مطلوب بالتفصيل.")
            .MaximumLength(StoreConsts.MaxAddressLength)
            .WithMessage($"العنوان لا يمكن أن يتجاوز {StoreConsts.MaxAddressLength} حرفاً.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0m, 90.0m)
            .WithMessage("خط العرض للمتجر يجب أن يكون بين -90 و 90 درجة.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0m, 180.0m)
            .WithMessage("خط الطول للمتجر يجب أن يكون بين -180 و 180 درجة.");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("الحد الأدنى لقيمة الطلب لا يمكن أن يكون سالباً.");

        // RuleFor(x => x.DeliveryFee)
        //     .GreaterThanOrEqualTo(0)
        //     .WithMessage("رسوم التوصيل الأساسية لا يمكن أن تكون سالبة.");

        When(x => x.AverageDeliveryTime.HasValue, () =>
        {
            RuleFor(x => x.AverageDeliveryTime!.Value)
                .GreaterThan(0)
                .WithMessage("متوسط وقت التوصيل بالدقائق يجب أن يكون أكبر من صفر.")
                .LessThanOrEqualTo(1440)
                .WithMessage("متوسط وقت التوصيل لا يمكن أن يتجاوز 1440 دقيقة (24 ساعة).");
        });
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق الصارمة لتعديل بيانات المتجر
/// </summary>
public class UpdateStoreDtoValidator : AbstractValidator<UpdateStoreDto>
{
    public UpdateStoreDtoValidator()
    {
        #region Rules
        RuleFor(x => x.StoreTypeId)
            .NotEmpty()
            .WithMessage("معرف نوع المتجر مطلوب.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("اسم المتجر مطلوب.")
            .MaximumLength(StoreConsts.MaxNameLength)
            .WithMessage($"اسم المتجر لا يمكن أن يتجاوز {StoreConsts.MaxNameLength} حرفاً.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("رقم الهاتف مطلوب.")
            .MaximumLength(StoreConsts.MaxPhoneLength)
            .WithMessage($"رقم الهاتف لا يمكن أن يتجاوز {StoreConsts.MaxPhoneLength} رقماً.")
            .Matches(@"^\+?[0-9]{7,15}$")
            .WithMessage("رقم الهاتف غير صالح، يجب أن يتكون من 7 إلى 15 رقماً.");

        When(x => !string.IsNullOrEmpty(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("صيغة البريد الإلكتروني للمتجر غير صحيحة.")
                .MaximumLength(StoreConsts.MaxEmailLength)
                .WithMessage($"البريد الإلكتروني لا يمكن أن يتجاوز {StoreConsts.MaxEmailLength} حرفاً.");
        });

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("عنوان المتجر مطلوب.")
            .MaximumLength(StoreConsts.MaxAddressLength)
            .WithMessage($"العنوان لا يمكن أن يتجاوز {StoreConsts.MaxAddressLength} حرفاً.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0m, 90.0m)
            .WithMessage("خط العرض للمتجر يجب أن يكون بين -90 و 90 درجة.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0m, 180.0m)
            .WithMessage("خط الطول للمتجر يجب أن يكون بين -180 و 180 درجة.");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("الحد الأدنى للطلب لا يمكن أن يكون سالباً.");

        // RuleFor(x => x.DeliveryFee)
        //     .GreaterThanOrEqualTo(0)
        //     .WithMessage("رسوم التوصيل لا يمكن أن تكون سالبة.");

        When(x => x.AverageDeliveryTime.HasValue, () =>
        {
            RuleFor(x => x.AverageDeliveryTime!.Value)
                .GreaterThan(0)
                .WithMessage("متوسط وقت التوصيل يجب أن يكون أكبر من صفر.")
                .LessThanOrEqualTo(1440)
                .WithMessage("متوسط وقت التوصيل لا يمكن أن يتجاوز 1440 دقيقة.");
        });
        #endregion
    }
}
#endregion

#region Store Type & Store User Validators
/// <summary>
/// محدد قواعد التحقق لإنشاء أنواع المتاجر
/// </summary>
public class CreateStoreTypeDtoValidator : AbstractValidator<CreateStoreTypeDto>
{
    public CreateStoreTypeDtoValidator()
    {
        #region Rules
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("اسم نوع المتجر إلزامي.")
            .MaximumLength(StoreTypeConsts.MaxNameLength)
            .WithMessage($"اسم نوع المتجر لا يتجاوز {StoreTypeConsts.MaxNameLength} حرفاً.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ترتيب العرض يجب أن يكون صفراً أو رقماً موجباً.");
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لتعديل أنواع المتاجر
/// </summary>
public class UpdateStoreTypeDtoValidator : AbstractValidator<UpdateStoreTypeDto>
{
    public UpdateStoreTypeDtoValidator()
    {
        #region Rules
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("اسم نوع المتجر إلزامي.")
            .MaximumLength(StoreTypeConsts.MaxNameLength)
            .WithMessage($"اسم نوع المتجر لا يتجاوز {StoreTypeConsts.MaxNameLength} حرفاً.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ترتيب العرض يجب أن يكون صفراً أو رقماً موجباً.");
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لإضافة موظف للمتجر
/// </summary>
public class CreateStoreUserDtoValidator : AbstractValidator<CreateStoreUserDto>
{
    public CreateStoreUserDtoValidator()
    {
        #region Rules
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithMessage("معرف المتجر إلزامي.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("معرف المستخدم إلزامي.");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("دور الموظف المحدد غير صالح.");
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لفلترة والبحث في قائمة المتاجر
/// </summary>
public class GetStoreListInputValidator : AbstractValidator<GetStoreListInput>
{
    public GetStoreListInputValidator()
    {
        #region Rules
        When(x => x.MinRating.HasValue, () =>
        {
            RuleFor(x => x.MinRating!.Value)
                .InclusiveBetween(1.0m, 5.0m)
                .WithMessage("أدنى تقييم للبحث يجب أن يكون بين 1.0 و 5.0 نجوم.");
        });

        RuleFor(x => x.MaxResultCount)
            .InclusiveBetween(1, 100)
            .WithMessage("عدد عناصر الصفحة يجب أن يتراوح بين 1 و 100 متجر.");
        #endregion
    }
}
#endregion
