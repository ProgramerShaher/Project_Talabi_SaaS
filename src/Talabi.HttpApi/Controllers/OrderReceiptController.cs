using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabi.MediaFiles;
using Talabi.MediaFiles.Dtos;
using Talabi.Orders;
using Talabi.Orders.Dtos;
using Volo.Abp;
using Volo.Abp.Content;

namespace Talabi.Controllers;

/// <summary>
/// متحكم إضافي لعمليات الطلبات التي تتطلب رفع ملفات مباشرة من الجهاز
/// </summary>
[Authorize]
[Route("api/app/order")]
public class OrderReceiptController : TalabiController
{
    private readonly IOrderAppService _orderAppService;
    private readonly IMediaFileAppService _mediaFileAppService;

    public OrderReceiptController(
        IOrderAppService orderAppService,
        IMediaFileAppService mediaFileAppService)
    {
        _orderAppService = orderAppService;
        _mediaFileAppService = mediaFileAppService;
    }

    /// <summary>
    /// رفع صورة إيصال الدفع للطلب مباشرة من جهازك واختيارها من المتصفح في Swagger
    /// </summary>
    /// <param name="id">معرف الطلب</param>
    /// <param name="receiptFile">صورة إيصال التحويل المالي أو البنكي المختارة من الجهاز</param>
    /// <returns>بيانات الطلب بعد تحديث رابط صورة الإيصال</returns>
    [HttpPost("{id}/upload-receipt")]
    [Consumes("multipart/form-data")]
    public async Task<OrderDto> UploadReceiptAsync(Guid id, IFormFile receiptFile)
    {
        if (receiptFile == null || receiptFile.Length == 0)
        {
            throw new UserFriendlyException("يرجى اختيار صورة إيصال الدفع لرفعها من جهازك.");
        }

        var stream = new RemoteStreamContent(
            receiptFile.OpenReadStream(),
            receiptFile.FileName,
            receiptFile.ContentType,
            receiptFile.Length
        );

        var mediaFile = await _mediaFileAppService.UploadAsync(new UploadMediaFileInput
        {
            File = stream,
            EntityType = "OrderReceipt",
            EntityId = id,
            IsPublic = true
        });

        return await _orderAppService.AttachPaymentReceiptAsync(id, mediaFile.PublicUrl ?? string.Empty, mediaFile.Id);
    }
}
