using System;
using FluentValidation;
using Talabi.Deliveries;
using Talabi.Deliveries.Dtos;

namespace Talabi.Deliveries.Validators;

#region Courier Profile & Tracking Validators
/// <summary>
/// محدد قواعد التحقق لتسجيل مندوب توصيل جديد
/// </summary>
public class CreateCourierDtoValidator : AbstractValidator<CreateCourierDto>
{
    public CreateCourierDtoValidator()
    {
        #region Rules
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("معرف المستخدم للمندوب إلزامي.");

        When(x => !string.IsNullOrEmpty(x.VehicleType), () =>
        {
            RuleFor(x => x.VehicleType)
                .MaximumLength(CourierConsts.MaxVehicleTypeLength)
                .WithMessage($"نوع المركبة لا يتجاوز {CourierConsts.MaxVehicleTypeLength} حرفاً.");
        });

        When(x => !string.IsNullOrEmpty(x.VehicleNumber), () =>
        {
            RuleFor(x => x.VehicleNumber)
                .MaximumLength(CourierConsts.MaxVehicleNumberLength)
                .WithMessage($"رقم لوحة المركبة لا يتجاوز {CourierConsts.MaxVehicleNumberLength} حرفاً.");
        });

        When(x => !string.IsNullOrEmpty(x.LicenseNumber), () =>
        {
            RuleFor(x => x.LicenseNumber)
                .MaximumLength(CourierConsts.MaxLicenseNumberLength)
                .WithMessage($"رقم رخصة القيادة لا يتجاوز {CourierConsts.MaxLicenseNumberLength} حرفاً.");
        });
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لتحديث موقع المندوب اللحظي عبر GPS
/// </summary>
public class UpdateCourierLocationInputValidator : AbstractValidator<UpdateCourierLocationInput>
{
    public UpdateCourierLocationInputValidator()
    {
        #region Rules
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0m, 90.0m)
            .WithMessage("خط العرض للمندوب يجب أن يكون بين -90 و 90 درجة.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0m, 180.0m)
            .WithMessage("خط الطول للمندوب يجب أن يكون بين -180 و 180 درجة.");
        #endregion
    }
}
#endregion

#region Assignment & Confirmation Validators
/// <summary>
/// محدد قواعد التحقق لتعيين وإسناد طلب لمندوب
/// </summary>
public class AssignCourierInputValidator : AbstractValidator<AssignCourierInput>
{
    public AssignCourierInputValidator()
    {
        #region Rules
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("معرف الطلب المراد إسناده إلزامي.");

        RuleFor(x => x.CourierId)
            .NotEmpty()
            .WithMessage("معرف المندوب المسند إليه إلزامي.");
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق الصارمة لتأكيد وإثبات تسليم الطلب
/// </summary>
public class ConfirmDeliveryInputValidator : AbstractValidator<ConfirmDeliveryInput>
{
    public ConfirmDeliveryInputValidator()
    {
        #region Rules
        RuleFor(x => x.DeliveryAssignmentId)
            .NotEmpty()
            .WithMessage("معرف مهمة التوصيل إلزامي للتأكيد.");

        RuleFor(x => x.ConfirmedBy)
            .IsInEnum()
            .WithMessage("طريقة إثبات وتأكيد التسليم غير صالحة.");

        When(x => !string.IsNullOrEmpty(x.VerificationCode), () =>
        {
            RuleFor(x => x.VerificationCode)
                .Matches(@"^[0-9]{4,8}$")
                .WithMessage("رمز التأكيد (OTP) يجب أن يتكون من 4 إلى 8 أرقام فقط.");
        });

        When(x => !string.IsNullOrEmpty(x.SignatureUrl), () =>
        {
            RuleFor(x => x.SignatureUrl)
                .MaximumLength(DeliveryConfirmationConsts.MaxSignatureUrlLength)
                .WithMessage($"رابط التوقيع لا يتجاوز {DeliveryConfirmationConsts.MaxSignatureUrlLength} حرفاً.");
        });

        When(x => !string.IsNullOrEmpty(x.PhotoProofUrl), () =>
        {
            RuleFor(x => x.PhotoProofUrl)
                .MaximumLength(DeliveryConfirmationConsts.MaxPhotoProofUrlLength)
                .WithMessage($"رابط صورة الإثبات لا يتجاوز {DeliveryConfirmationConsts.MaxPhotoProofUrlLength} حرفاً.");
        });

        When(x => !string.IsNullOrEmpty(x.Notes), () =>
        {
            RuleFor(x => x.Notes)
                .MaximumLength(DeliveryConfirmationConsts.MaxNotesLength)
                .WithMessage($"ملاحظات التسليم لا تتجاوز {DeliveryConfirmationConsts.MaxNotesLength} حرفاً.");
        });
        #endregion
    }
}
#endregion
