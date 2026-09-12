using System;
using FluentValidation;
using Talabi.Payments;
using Talabi.Payments.Dtos;

namespace Talabi.Payments.Validators;

#region Payment Receipt Validators
/// <summary>
/// محدد قواعد التحقق الصارمة لإرسال إيصال دفع حوالة/محفظة
/// </summary>
public class SubmitPaymentReceiptInputValidator : AbstractValidator<SubmitPaymentReceiptInput>
{
    public SubmitPaymentReceiptInputValidator()
    {
        #region Rules
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("معرف عملية الدفع (PaymentId) إلزامي.");

        RuleFor(x => x.MediaFileId)
            .NotEmpty()
            .WithMessage("معرف ملف الإيصال المرفوع (MediaFileId) إلزامي.");

        When(x => !string.IsNullOrEmpty(x.WalletName), () =>
        {
            RuleFor(x => x.WalletName)
                .MaximumLength(PaymentReceiptConsts.MaxWalletNameLength)
                .WithMessage($"اسم المحفظة/البنك لا يتجاوز {PaymentReceiptConsts.MaxWalletNameLength} حرفاً.");
        });

        When(x => !string.IsNullOrEmpty(x.TransactionNumber), () =>
        {
            RuleFor(x => x.TransactionNumber)
                .MaximumLength(PaymentReceiptConsts.MaxTransactionNumberLength)
                .WithMessage($"رقم الحوالة/العملية لا يتجاوز {PaymentReceiptConsts.MaxTransactionNumberLength} حرفاً.");
        });

        When(x => x.Amount.HasValue, () =>
        {
            RuleFor(x => x.Amount!.Value)
                .GreaterThan(0)
                .WithMessage("المبلغ المدفوع في الإيصال يجب أن يكون أكبر من صفر.");
        });
        #endregion
    }
}

/// <summary>
/// محدد قواعد التحقق لمراجعة واعتماد أو رفض إيصال الدفع
/// </summary>
public class VerifyPaymentReceiptInputValidator : AbstractValidator<VerifyPaymentReceiptInput>
{
    public VerifyPaymentReceiptInputValidator()
    {
        #region Rules
        RuleFor(x => x.ReceiptId)
            .NotEmpty()
            .WithMessage("معرف الإيصال إلزامي للتحقق.");

        When(x => !x.IsApproved, () =>
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .WithMessage("يجب توضيح سبب رفض إيصال الدفع للعميل بشكل صريح.")
                .MaximumLength(PaymentReceiptConsts.MaxRejectionReasonLength)
                .WithMessage($"سبب رفض الإيصال لا يتجاوز {PaymentReceiptConsts.MaxRejectionReasonLength} حرفاً.");
        });
        #endregion
    }
}
#endregion
