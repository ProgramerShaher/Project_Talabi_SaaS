using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;

namespace Talabi.MediaFiles.Dtos;

/// <summary>
/// كائن عرض بيانات الملف أو الصورة
/// </summary>
public class MediaFileDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public string FileName { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Extension { get; set; }
    public string StorageProvider { get; set; } = string.Empty;
    public string? PublicUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? MediumUrl { get; set; }
    public string? OptimizedUrl { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public bool IsPublic { get; set; }
    public MediaAccessLevel AccessLevel { get; set; }
    public MediaProcessingStatus Status { get; set; }
    public DateTime? UploadedAt { get; set; }
    #endregion
}

/// <summary>
/// كائن استقبال رفع ملف جديد في ABP (باستخدام IRemoteStreamContent المدعوم رسمياً من ABP)
/// </summary>
public class UploadMediaFileInput
{
    #region Properties
    /// <summary>
    /// محتوى الملف المرفوع
    /// </summary>
    [Required(ErrorMessage = "الملف مطلوب")]
    public IRemoteStreamContent File { get; set; } = null!;

    /// <summary>
    /// نوع الكيان المرتبط (Product, Store, Receipt)
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// معرف الكيان المرتبط
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// هل الملف عام ومتاح للجميع؟
    /// </summary>
    public bool IsPublic { get; set; } = true;
    #endregion
}
