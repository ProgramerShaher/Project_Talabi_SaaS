using System;
using FluentValidation;
using Talabi.Interactions;
using Talabi.Interactions.Dtos;

namespace Talabi.Interactions.Validators;

#region Interaction Validators
/// <summary>
/// محدد قواعد التحقق الصارمة لتقييم ومراجعة الأنشطة التجارية
/// </summary>
public class CreateReviewInputValidator : AbstractValidator<CreateReviewInput>
{
    public CreateReviewInputValidator()
    {
        #region Rules
        // RuleFor(x => x.OrderId)
        //     .NotEmpty()
        //     .WithMessage("معرف الطلب إلزامي لربط التقييم به.");

        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithMessage("معرف النشاط التجاري إلزامي لربط التقييم به.");

        RuleFor(x => x.StoreRating)
            .InclusiveBetween(ReviewConsts.MinRating, ReviewConsts.MaxRating)
            .WithMessage($"تقييم المتجر إلزامي ويجب أن يتراوح بين {ReviewConsts.MinRating} و {ReviewConsts.MaxRating} نجوم.");

        // When(x => x.CourierRating.HasValue, () =>
        // {
        //     RuleFor(x => x.CourierRating!.Value)
        //         .InclusiveBetween(ReviewConsts.MinRating, ReviewConsts.MaxRating)
        //         .WithMessage($"تقييم مندوب التوصيل يجب أن يتراوح بين {ReviewConsts.MinRating} و {ReviewConsts.MaxRating} نجوم.");
        // });

        When(x => x.ProductQualityRating.HasValue, () =>
        {
            RuleFor(x => x.ProductQualityRating!.Value)
                .InclusiveBetween(ReviewConsts.MinRating, ReviewConsts.MaxRating)
                .WithMessage($"تقييم جودة المنتج يجب أن يتراوح بين {ReviewConsts.MinRating} و {ReviewConsts.MaxRating} نجوم.");
        });

        // When(x => x.DeliverySpeedRating.HasValue, () =>
        // {
        //     RuleFor(x => x.DeliverySpeedRating!.Value)
        //         .InclusiveBetween(ReviewConsts.MinRating, ReviewConsts.MaxRating)
        //         .WithMessage($"تقييم سرعة التوصيل يجب أن يتراوح بين {ReviewConsts.MinRating} و {ReviewConsts.MaxRating} نجوم.");
        // });

        When(x => !string.IsNullOrEmpty(x.Comment), () =>
        {
            RuleFor(x => x.Comment)
                .MaximumLength(ReviewConsts.MaxCommentLength)
                .WithMessage($"التعليق النصي للتقييم لا يمكن أن يتجاوز {ReviewConsts.MaxCommentLength} حرفاً.");
        });
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لتعديل تقييم المتجر
/// </summary>
public class UpdateReviewInputValidator : AbstractValidator<UpdateReviewInput>
{
    public UpdateReviewInputValidator()
    {
        #region Rules
        RuleFor(x => x.StoreRating)
            .InclusiveBetween(ReviewConsts.MinRating, ReviewConsts.MaxRating)
            .WithMessage($"تقييم المتجر إلزامي ويجب أن يتراوح بين {ReviewConsts.MinRating} و {ReviewConsts.MaxRating} نجوم.");

        When(x => x.ProductQualityRating.HasValue, () =>
        {
            RuleFor(x => x.ProductQualityRating!.Value)
                .InclusiveBetween(ReviewConsts.MinRating, ReviewConsts.MaxRating)
                .WithMessage($"تقييم جودة المنتج يجب أن يتراوح بين {ReviewConsts.MinRating} و {ReviewConsts.MaxRating} نجوم.");
        });

        When(x => !string.IsNullOrEmpty(x.Comment), () =>
        {
            RuleFor(x => x.Comment)
                .MaximumLength(ReviewConsts.MaxCommentLength)
                .WithMessage($"التعليق النصي للتقييم لا يمكن أن يتجاوز {ReviewConsts.MaxCommentLength} حرفاً.");
        });
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لإضافة أو إزالة عناصر من المفضلة
/// </summary>
public class ToggleFavoriteInputValidator : AbstractValidator<ToggleFavoriteInput>
{
    public ToggleFavoriteInputValidator()
    {
        #region Rules
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithMessage("نوع الكيان المفضل (مثل: Product, Store) إلزامي.")
            .MaximumLength(FavoriteConsts.MaxEntityTypeLength)
            .WithMessage($"نوع الكيان لا يتجاوز {FavoriteConsts.MaxEntityTypeLength} حرفاً.")
            .Matches(@"^[a-zA-Z0-9_\-]+$")
            .WithMessage("نوع الكيان يجب أن يحتوي على أحرف وأرقام فقط دون رموز خاصة.");

        RuleFor(x => x.EntityId)
            .NotEmpty()
            .WithMessage("معرف الكيان المفضل إلزامي.");
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لفلترة والبحث في قائمة التقييمات
/// </summary>
public class GetReviewListInputValidator : AbstractValidator<GetReviewListInput>
{
    public GetReviewListInputValidator()
    {
        #region Rules
        When(x => x.MinRating.HasValue, () =>
        {
            RuleFor(x => x.MinRating!.Value)
                .InclusiveBetween(1, 5)
                .WithMessage("الحد الأدنى للتقييم في البحث يجب أن يكون بين 1 و 5 نجوم.");
        });

        RuleFor(x => x.MaxResultCount)
            .InclusiveBetween(1, 100)
            .WithMessage("عدد عناصر الصفحة يجب أن يتراوح بين 1 و 100 تقييم.");
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لرد المتجر الرسمي على التقييم
/// </summary>
public class StoreReplyInputValidator : AbstractValidator<StoreReplyInput>
{
    public StoreReplyInputValidator()
    {
        #region Rules
        RuleFor(x => x.Reply)
            .NotEmpty()
            .WithMessage("نص رد المتجر إلزامي ولا يمكن تركه فارغاً.")
            .MaximumLength(ReviewConsts.MaxStoreReplyLength)
            .WithMessage($"رد المتجر لا يمكن أن يتجاوز {ReviewConsts.MaxStoreReplyLength} حرفاً.");
        #endregion
    }
}
#endregion
