using System;
using FluentValidation;
using Talabi.Products;
using Talabi.Products.Dtos;

namespace Talabi.Products.Validators;

#region Product Validators
/// <summary>
/// محدد قواعد التحقق الصارمة لإضافة منتج جديد
/// </summary>
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        #region Rules
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithMessage("معرف المتجر إلزامي لربط المنتج به.");

        RuleFor(x => x.StoreCategoryId)
            .NotEmpty()
            .WithMessage("معرف تصنيف المتجر إلزامي لربط المنتج به.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("اسم المنتج مطلوب.")
            .MaximumLength(ProductConsts.MaxNameLength)
            .WithMessage($"اسم المنتج لا يمكن أن يتجاوز {ProductConsts.MaxNameLength} حرفاً.");

        RuleFor(x => x.SKU)
            .NotEmpty()
            .WithMessage("رمز المنتج (SKU) مطلوب للتمييز وتتبع المخزون.")
            .MaximumLength(ProductConsts.MaxSkuLength)
            .WithMessage($"رمز المنتج لا يمكن أن يتجاوز {ProductConsts.MaxSkuLength} حرفاً.")
            .Matches(@"^[A-Za-z0-9_\-]+$")
            .WithMessage("رمز المنتج SKU يجب أن يحتوي فقط على أحرف وأرقام وشرطة أو شرطة سفلية.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("سعر المنتج الأساسي يجب أن يكون أكبر من صفر.");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("قيمة الخصم لا يمكن أن تكون سالبة.");

        When(x => x.DiscountType == DiscountType.Percentage, () =>
        {
            RuleFor(x => x.Discount)
                .InclusiveBetween(0, 100)
                .WithMessage("نسبة الخصم المئوية يجب أن تكون محصورة بين 0% و 100%.");
        });

        When(x => x.DiscountType == DiscountType.Fixed, () =>
        {
            RuleFor(x => x.Discount)
                .LessThan(x => x.Price)
                .WithMessage("قيمة الخصم الثابت يجب أن تكون أقل من سعر المنتج الأساسي.");
        });

        // When(x => x.CostPrice.HasValue, () =>
        // {
        //     RuleFor(x => x.CostPrice!.Value)
        //         .GreaterThanOrEqualTo(0)
        //         .WithMessage("سعر التكلفة لا يمكن أن يكون سالباً.");
        // });

        RuleFor(x => x.Unit)
            .NotEmpty()
            .WithMessage("وحدة القياس للمنتج مطلوبة (مثل: حبة، كجم، كرتون).")
            .MaximumLength(ProductConsts.MaxUnitLength)
            .WithMessage($"وحدة القياس لا تتجاوز {ProductConsts.MaxUnitLength} حرفاً.");

        RuleFor(x => x.MinOrderQuantity)
            .GreaterThanOrEqualTo(1)
            .WithMessage("الحد الأدنى للطلب يجب أن يكون 1 على الأقل.");

        When(x => x.MaxOrderQuantity.HasValue, () =>
        {
            RuleFor(x => x.MaxOrderQuantity!.Value)
                .GreaterThanOrEqualTo(x => x.MinOrderQuantity)
                .WithMessage("الحد الأقصى للطلب يجب أن يكون أكبر من أو يساوي الحد الأدنى للطلب.");
        });

        // RuleFor(x => x.InitialQuantity)
        //     .GreaterThanOrEqualTo(0)
        //     .WithMessage("الكمية الافتتاحية للمخزون يجب أن تكون صفراً أو أكثر.");

        // When(x => x.Weight.HasValue, () =>
        // {
        //     RuleFor(x => x.Weight!.Value)
        //         .GreaterThan(0)
        //         .WithMessage("الوزن يجب أن يكون قيمة موجبة أكبر من صفر.");
        // });
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق الصارمة لتعديل بيانات المنتج
/// </summary>
public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        #region Rules
        RuleFor(x => x.StoreCategoryId)
            .NotEmpty()
            .WithMessage("معرف تصنيف المتجر مطلوب.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("اسم المنتج مطلوب.")
            .MaximumLength(ProductConsts.MaxNameLength)
            .WithMessage($"اسم المنتج لا يمكن أن يتجاوز {ProductConsts.MaxNameLength} حرفاً.");

        RuleFor(x => x.SKU)
            .NotEmpty()
            .WithMessage("رمز المنتج (SKU) مطلوب.")
            .MaximumLength(ProductConsts.MaxSkuLength)
            .WithMessage($"رمز المنتج لا يمكن أن يتجاوز {ProductConsts.MaxSkuLength} حرفاً.")
            .Matches(@"^[A-Za-z0-9_\-]+$")
            .WithMessage("رمز المنتج SKU يجب أن يحتوي فقط على أحرف وأرقام وشرطة أو شرطة سفلية.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("سعر المنتج الأساسي يجب أن يكون أكبر من صفر.");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("قيمة الخصم لا يمكن أن تكون سالبة.");

        When(x => x.DiscountType == DiscountType.Percentage, () =>
        {
            RuleFor(x => x.Discount)
                .InclusiveBetween(0, 100)
                .WithMessage("نسبة الخصم المئوية يجب أن تكون بين 0% و 100%.");
        });

        When(x => x.DiscountType == DiscountType.Fixed, () =>
        {
            RuleFor(x => x.Discount)
                .LessThan(x => x.Price)
                .WithMessage("قيمة الخصم الثابت يجب أن تكون أقل من سعر المنتج الأساسي.");
        });

        // When(x => x.CostPrice.HasValue, () =>
        // {
        //     RuleFor(x => x.CostPrice!.Value)
        //         .GreaterThanOrEqualTo(0)
        //         .WithMessage("سعر التكلفة لا يمكن أن يكون سالباً.");
        // });

        RuleFor(x => x.Unit)
            .NotEmpty()
            .WithMessage("وحدة القياس للمنتج مطلوبة.")
            .MaximumLength(ProductConsts.MaxUnitLength)
            .WithMessage($"وحدة القياس لا تتجاوز {ProductConsts.MaxUnitLength} حرفاً.");

        RuleFor(x => x.MinOrderQuantity)
            .GreaterThanOrEqualTo(1)
            .WithMessage("الحد الأدنى للطلب يجب أن يكون 1 على الأقل.");

        When(x => x.MaxOrderQuantity.HasValue, () =>
        {
            RuleFor(x => x.MaxOrderQuantity!.Value)
                .GreaterThanOrEqualTo(x => x.MinOrderQuantity)
                .WithMessage("الحد الأقصى للطلب يجب أن يكون أكبر من أو يساوي الحد الأدنى للطلب.");
        });

        // When(x => x.Weight.HasValue, () =>
        // {
        //     RuleFor(x => x.Weight!.Value)
        //         .GreaterThan(0)
        //         .WithMessage("الوزن يجب أن يكون قيمة موجبة أكبر من صفر.");
        // });
        #endregion
    }
}
#endregion

#region Product Image & Inventory Validators
/// <summary>
/// محدد قواعد التحقق لصور المنتج
/// </summary>
public class CreateProductImageDtoValidator : AbstractValidator<CreateProductImageDto>
{
    public CreateProductImageDtoValidator()
    {
        #region Rules
        RuleFor(x => x.ImageUrl)
            .NotEmpty()
            .WithMessage("رابط صورة المنتج مطلوب.")
            .MaximumLength(ProductImageConsts.MaxImageUrlLength)
            .WithMessage($"رابط الصورة لا يمكن أن يتجاوز {ProductImageConsts.MaxImageUrlLength} حرفاً.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكبر.");
        #endregion
    }
}

// /// <summary>
// /// محدد قواعد التحقق للمخزون - معطل
// /// </summary>
// public class UpdateInventoryDtoValidator : AbstractValidator<UpdateInventoryDto>
// {
//     public UpdateInventoryDtoValidator()
//     {
//         #region Rules
//         RuleFor(x => x.CurrentQuantity)
//             .GreaterThanOrEqualTo(0)
//             .WithMessage("الكمية الحالية في المخزون لا يمكن أن تكون سالبة.");
// 
//         RuleFor(x => x.MinStockLevel)
//             .GreaterThanOrEqualTo(0)
//             .WithMessage("الحد الأدنى للمخزون لا يمكن أن يكون سالباً.");
// 
//         RuleFor(x => x.ReorderLevel)
//             .GreaterThanOrEqualTo(0)
//             .WithMessage("نقطة إعادة الطلب لا يمكن أن تكون سالبة.");
// 
//         RuleFor(x => x.MaxStockLevel)
//             .GreaterThanOrEqualTo(x => x.MinStockLevel)
//             .WithMessage("الحد الأقصى للمخزون يجب أن يكون أكبر من أو يساوي الحد الأدنى للمخزون.");
//         #endregion
//     }
// }

/// <summary>
/// محدد قواعد التحقق لفلترة والبحث في قائمة المنتجات
/// </summary>
public class GetProductListInputValidator : AbstractValidator<GetProductListInput>
{
    public GetProductListInputValidator()
    {
        #region Rules
        When(x => x.MinPrice.HasValue, () =>
        {
            RuleFor(x => x.MinPrice!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("الحد الأدنى للسعر لا يمكن أن يكون سالباً.");
        });

        When(x => x.MaxPrice.HasValue, () =>
        {
            RuleFor(x => x.MaxPrice!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("الحد الأقصى للسعر لا يمكن أن يكون سالباً.");
        });

        When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue, () =>
        {
            RuleFor(x => x.MaxPrice!.Value)
                .GreaterThanOrEqualTo(x => x.MinPrice!.Value)
                .WithMessage("الحد الأقصى للسعر يجب أن يكون أكبر من أو يساوي الحد الأدنى للسعر.");
        });

        RuleFor(x => x.MaxResultCount)
            .InclusiveBetween(1, 100)
            .WithMessage("عدد عناصر الصفحة يجب أن يتراوح بين 1 و 100 عنصر.");
        #endregion
    }
}
#endregion
