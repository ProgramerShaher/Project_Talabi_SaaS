using System;
using FluentValidation;
using Talabi.Categories;
using Talabi.Categories.Dtos;

namespace Talabi.Categories.Validators;

#region Global Category Validators
/// <summary>
/// محدد قواعد التحقق لإنشاء تصنيف عام
/// </summary>
public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        #region Rules
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("اسم التصنيف العام مطلوب.")
            .MaximumLength(CategoryConsts.MaxNameLength)
            .WithMessage($"اسم التصنيف لا يمكن أن يتجاوز {CategoryConsts.MaxNameLength} حرفاً.");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("الرابط اللطيف (Slug) مطلوب.")
            .MaximumLength(CategoryConsts.MaxSlugLength)
            .WithMessage($"الرابط اللطيف لا يمكن أن يتجاوز {CategoryConsts.MaxSlugLength} حرفاً.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("صيغة الرابط اللطيف غير صالحة، يجب أن تحتوي على أحرف إنجليزية صغيرة وأرقام ومفصولة بشرطة.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكبر.");
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لتعديل تصنيف عام
/// </summary>
public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        #region Rules
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("اسم التصنيف العام مطلوب.")
            .MaximumLength(CategoryConsts.MaxNameLength)
            .WithMessage($"اسم التصنيف لا يمكن أن يتجاوز {CategoryConsts.MaxNameLength} حرفاً.");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("الرابط اللطيف مطلوب.")
            .MaximumLength(CategoryConsts.MaxSlugLength)
            .WithMessage($"الرابط اللطيف لا يمكن أن يتجاوز {CategoryConsts.MaxSlugLength} حرفاً.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("صيغة الرابط اللطيف غير صالحة.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكبر.");
        #endregion
    }
}
#endregion

#region Store Category Validators
/// <summary>
/// محدد قواعد التحقق لتصنيفات المتجر الداخلية
/// </summary>
public class CreateUpdateStoreCategoryDtoValidator : AbstractValidator<CreateUpdateStoreCategoryDto>
{
    public CreateUpdateStoreCategoryDtoValidator()
    {
        #region Rules
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithMessage("معرف المتجر (StoreId) مطلوب لربط التصنيف.");

        When(x => !string.IsNullOrEmpty(x.CustomName), () =>
        {
            RuleFor(x => x.CustomName)
                .MaximumLength(StoreCategoryConsts.MaxCustomNameLength)
                .WithMessage($"الاسم المخصص للتصنيف لا يمكن أن يتجاوز {StoreCategoryConsts.MaxCustomNameLength} حرفاً.");
        });

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكبر.");
        #endregion
    }
}
#endregion
