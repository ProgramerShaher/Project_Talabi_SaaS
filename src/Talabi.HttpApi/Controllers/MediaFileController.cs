using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabi.MediaFiles;
using Talabi.MediaFiles.Dtos;
using Volo.Abp;
using Volo.Abp.Content;

namespace Talabi.Controllers;

/// <summary>
/// متحكم إدارة ورفع الملفات والوسائط الرقمية
/// </summary>
[Authorize]
[Route("api/app/media-file")]
public class MediaFileController : TalabiController
{
    private readonly IMediaFileAppService _mediaFileAppService;

    public MediaFileController(IMediaFileAppService mediaFileAppService)
    {
        _mediaFileAppService = mediaFileAppService;
    }

    /// <summary>
    /// رفع ملف أو صورة مباشرة من جهاز المستخدم عبر نافذة اختيار الملفات في Swagger
    /// </summary>
    /// <param name="file">الملف أو الصورة المختارة من الجهاز</param>
    /// <param name="entityType">نوع الكيان المرتبط (مثل Receipt, Store, Product)</param>
    /// <param name="entityId">معرف الكيان المرتبط إن وجد</param>
    /// <param name="isPublic">هل الملف متاح للعامة</param>
    /// <returns>تفاصيل الملف المرفوع والرابط المباشر للوصول إليه</returns>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<MediaFileDto> UploadAsync(
        IFormFile file,
        [FromForm] string? entityType = null,
        [FromForm] Guid? entityId = null,
        [FromForm] bool isPublic = true)
    {
        if (file == null || file.Length == 0)
        {
            throw new UserFriendlyException("يرجى اختيار ملف لرفعه من الجهاز.");
        }

        var stream = new RemoteStreamContent(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            file.Length
        );

        return await _mediaFileAppService.UploadAsync(new UploadMediaFileInput
        {
            File = stream,
            EntityType = entityType,
            EntityId = entityId,
            IsPublic = isPublic
        });
    }

    /// <summary>
    /// جلب تفاصيل ملف مرفوع بواسطة المعرف
    /// </summary>
    [HttpGet("{id}")]
    public async Task<MediaFileDto> GetAsync(Guid id)
    {
        return await _mediaFileAppService.GetAsync(id);
    }

    /// <summary>
    /// عرض أو تحميل محتوى الملف مباشرة عبر اسمه
    /// </summary>
    [HttpGet("{fileName}/content")]
    [AllowAnonymous]
    public async Task<IActionResult> GetContentAsync(string fileName)
    {
        var content = await _mediaFileAppService.GetContentAsync(fileName);
        return File(content.GetStream(), content.ContentType, content.FileName);
    }

    /// <summary>
    /// حذف ملف من الخادم وقاعدة البيانات
    /// </summary>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await _mediaFileAppService.DeleteAsync(id);
    }
}
