using System;
using System.Threading.Tasks;
using Talabi.Payments.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Payments;

/// <summary>
/// واجهة خدمة مراجعة والتحقق من إيصالات الدفع والتحويلات المالية
/// </summary>
public interface IPaymentReceiptAppService : IApplicationService
{
    /// <summary>
    /// الحصول على تفاصيل إيصال دفع بواسطة معرفه الفريد
    /// </summary>
    Task<PaymentReceiptDetailsDto> GetAsync(Guid id);

    /// <summary>
    /// الحصول على تفاصيل إيصال الدفع لطلب محدد
    /// </summary>
    Task<PaymentReceiptDetailsDto?> GetByOrderIdAsync(Guid orderId);

    /// <summary>
    /// الحصول على تفاصيل إيصال الدفع لعملية دفع محددة
    /// </summary>
    Task<PaymentReceiptDetailsDto?> GetByPaymentIdAsync(Guid paymentId);

    /// <summary>
    /// استعراض وتصفية قائمة إيصالات الدفع لأصحاب المتاجر وإدارة النظام
    /// </summary>
    Task<PagedResultDto<PaymentReceiptDetailsDto>> GetListAsync(GetPaymentReceiptListInput input);

    /// <summary>
    /// تقديم إيصال دفع جديد أو إعادة رفع إيصال بعد الرفض
    /// </summary>
    Task<PaymentReceiptDetailsDto> SubmitReceiptAsync(SubmitPaymentReceiptInput input);

    /// <summary>
    /// مراجعة والتحقق من إيصال الدفع (قبول أو رفض مع تدوين السبب)
    /// </summary>
    Task<PaymentReceiptDetailsDto> VerifyReceiptAsync(VerifyPaymentReceiptInput input);
}
