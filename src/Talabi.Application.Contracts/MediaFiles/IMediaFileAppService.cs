using System;
using System.Threading.Tasks;
using Talabi.MediaFiles.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Talabi.MediaFiles;

/// <summary>
/// واجهة خدمة رفع وإدارة الملفات والوسائط الرقمية
/// </summary>
public interface IMediaFileAppService : IApplicationService
{
    /// <summary>
    /// رفع ملف جديد (صورة، ملف)
    /// </summary>
    /// <param name="input">بيانات الملف المرفوع</param>
    /// <returns>تفاصيل الملف المرفوع بما في ذلك الرابط</returns>
    Task<MediaFileDto> UploadAsync(UploadMediaFileInput input);

    /// <summary>
    /// جلب تفاصيل ملف باستخدام المعرف
    /// </summary>
    Task<MediaFileDto> GetAsync(Guid id);

    /// <summary>
    /// جلب محتوى الملف (للتحميل أو العرض المباشر)
    /// </summary>
    Task<IRemoteStreamContent> GetContentAsync(string fileName);

    /// <summary>
    /// حذف ملف من النظام والخادم
    /// </summary>
    Task DeleteAsync(Guid id);
}
