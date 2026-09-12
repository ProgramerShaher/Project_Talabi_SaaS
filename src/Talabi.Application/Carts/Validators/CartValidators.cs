using System;
using FluentValidation;
using Talabi.Carts;
using Talabi.Carts.Dtos;

namespace Talabi.Carts.Validators;

#region Cart Validators
/// <summary>
/// محدد قواعد التحقق الصارمة لإضافة عنصر إلى سلة المشتريات
/// </summary>
public class AddToCartInputValidator : AbstractValidator<AddToCartInput>
{
    public AddToCartInputValidator()
    {
        #region Rules
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithMessage("معرف المتجر إلزامي لإضافة عناصر إلى السلة.");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("معرف المنتج إلزامي لإضافته إلى السلة.");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 1000)
            .WithMessage("الكمية المضافة إلى السلة يجب أن تكون بين 1 و 1000 قطعة.");

        When(x => !string.IsNullOrEmpty(x.Notes), () =>
        {
            RuleFor(x => x.Notes)
                .MaximumLength(CartConsts.MaxNotesLength)
                .WithMessage($"ملاحظات المنتج في السلة لا يمكن أن تتجاوز {CartConsts.MaxNotesLength} حرفاً.");
        });
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لتعديل كمية عنصر في السلة
/// </summary>
public class UpdateCartItemQuantityInputValidator : AbstractValidator<UpdateCartItemQuantityInput>
{
    public UpdateCartItemQuantityInputValidator()
    {
        #region Rules
        RuleFor(x => x.Quantity)
            .InclusiveBetween(0, 1000)
            .WithMessage("كمية العنصر في السلة يجب أن تكون بين 0 و 1000 قطعة (الصفر يعني حذف العنصر).");
        #endregion
    }
}
#endregion
