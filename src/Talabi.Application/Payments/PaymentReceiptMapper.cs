using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Talabi.Payments.Dtos;

namespace Talabi.Payments;

/// <summary>
/// محول الكائنات التلقائي لبيانات إيصالات الدفع والتحويلات المالية عبر Mapperly
/// </summary>
[Mapper]
public partial class PaymentReceiptMapper
{
    /// <summary>
    /// تحويل كيان إيصال الدفع إلى كائن عرض PaymentReceiptDto
    /// </summary>
    [MapperIgnoreTarget(nameof(PaymentReceiptDto.ReceiptFileUrl))]
    public partial PaymentReceiptDto ToReceiptDto(PaymentReceipt source);

    /// <summary>
    /// تحويل قائمة إيصالات الدفع إلى قائمة PaymentReceiptDto
    /// </summary>
    [MapperIgnoreTarget(nameof(PaymentReceiptDto.ReceiptFileUrl))]
    public partial List<PaymentReceiptDto> ToReceiptDtoList(List<PaymentReceipt> source);
}
