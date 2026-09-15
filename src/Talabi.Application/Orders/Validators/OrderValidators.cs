using System;
using System.Linq;
using FluentValidation;
using Talabi.Orders;
using Talabi.Orders.Dtos;

namespace Talabi.Orders.Validators;

#region Order Creation Validators
/// <summary>
/// محدد قواعد التحقق الصارمة لإنشاء طلب جديد
/// </summary>
public class CreateOrderInputValidator : AbstractValidator<CreateOrderInput>
{
    public CreateOrderInputValidator()
    {
        #region Rules
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithMessage("معرف المتجر إلزامي لإنشاء الطلب.");

        RuleFor(x => x.DeliveryAddressId)
            .NotEmpty()
            .WithMessage("معرف عنوان التوصيل إلزامي لإنشاء الطلب.");

        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage("معرف طريقة الدفع إلزامي لإنشاء الطلب.");

        When(x => !string.IsNullOrEmpty(x.CustomerNotes), () =>
        {
            RuleFor(x => x.CustomerNotes)
                .MaximumLength(OrderConsts.MaxCustomerNotesLength)
                .WithMessage($"ملاحظات العميل على الطلب لا يمكن أن تتجاوز {OrderConsts.MaxCustomerNotesLength} حرفاً.");
        });

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("قائمة عناصر الطلب لا يمكن أن تكون فارغة.")
            .Must(items => items != null && items.Count <= 100)
            .WithMessage("لا يمكن أن يحتوي الطلب الواحد على أكثر من 100 صنف مختلف.")
            .Must(items => items == null || items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .WithMessage("يوجد تكرار لنفس المنتج في عناصر الطلب. يُرجى دمج الكميات لنفس المنتج.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemDtoValidator());
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لعنصر الطلب الفردي
/// </summary>
public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        #region Rules
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("معرف المنتج في عنصر الطلب إلزامي.");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 1000)
            .WithMessage("كمية المنتج في الطلب يجب أن تكون بين 1 و 1000 قطعة.");

        When(x => !string.IsNullOrEmpty(x.Notes), () =>
        {
            RuleFor(x => x.Notes)
                .MaximumLength(OrderItemConsts.MaxNotesLength)
                .WithMessage($"ملاحظات عنصر الطلب لا يمكن أن تتجاوز {OrderItemConsts.MaxNotesLength} حرفاً.");
        });
        #endregion
    }
}
#endregion

#region Order Decision Validators
/// <summary>
/// محدد قواعد التحقق لرفض طلب من المتجر
/// </summary>
public class RejectOrderInputValidator : AbstractValidator<RejectOrderInput>
{
    public RejectOrderInputValidator()
    {
        #region Rules
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("معرف الطلب المراد رفضه إلزامي.");

        RuleFor(x => x.RejectionReasonId)
            .NotEmpty()
            .WithMessage("معرف سبب الرفض إلزامي.");

        When(x => !string.IsNullOrEmpty(x.AdditionalNotes), () =>
        {
            RuleFor(x => x.AdditionalNotes)
                .MaximumLength(CancellationReasonConsts.MaxAdditionalNotesLength)
                .WithMessage($"الملاحظات الإضافية لرفض الطلب لا تتجاوز {CancellationReasonConsts.MaxAdditionalNotesLength} حرفاً.");
        });
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لإلغاء الطلب من العميل أو الإدارة
/// </summary>
public class CancelOrderInputValidator : AbstractValidator<CancelOrderInput>
{
    public CancelOrderInputValidator()
    {
        #region Rules
        When(x => !string.IsNullOrEmpty(x.AdditionalNotes), () =>
        {
            RuleFor(x => x.AdditionalNotes)
                .MaximumLength(CancellationReasonConsts.MaxAdditionalNotesLength)
                .WithMessage($"الملاحظات الإضافية لإلغاء الطلب لا تتجاوز {CancellationReasonConsts.MaxAdditionalNotesLength} حرفاً.");
        });
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لفلترة والبحث في قائمة الطلبات
/// </summary>
public class GetOrderListInputValidator : AbstractValidator<GetOrderListInput>
{
    public GetOrderListInputValidator()
    {
        #region Rules
        When(x => x.FromDate.HasValue && x.ToDate.HasValue, () =>
        {
            RuleFor(x => x.ToDate!.Value)
                .GreaterThanOrEqualTo(x => x.FromDate!.Value)
                .WithMessage("تاريخ نهاية البحث يجب أن يكون أكبر من أو يساوي تاريخ البداية.");
        });

        RuleFor(x => x.MaxResultCount)
            .InclusiveBetween(1, 100)
            .WithMessage("عدد عناصر الصفحة يجب أن يتراوح بين 1 و 100 عنصر.");
        #endregion
    }
}
#endregion
