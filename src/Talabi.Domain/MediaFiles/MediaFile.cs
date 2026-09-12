using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.MediaFiles;

/// <summary>
/// كيان إدارة وحفظ وتتبع الملفات والصور والوسائط في النظام
/// </summary>
public class MediaFile : FullAuditedEntity<Guid>
{
    /// <summary>
    /// اسم الملف المحفوظ في وسيط التخزين
    /// </summary>
    public virtual string FileName { get; set; } = string.Empty;

    /// <summary>
    /// اسم الملف الأصلي عند رفعه من جهاز المستخدم
    /// </summary>
    public virtual string? OriginalFileName { get; set; }

    /// <summary>
    /// نوع المحتوى MIME Type (مثل image/jpeg, application/pdf)
    /// </summary>
    public virtual string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// حجم الملف بالبايت
    /// </summary>
    public virtual long FileSize { get; set; }

    /// <summary>
    /// امتداد الملف (jpg, png, pdf)
    /// </summary>
    public virtual string? Extension { get; set; }

    /// <summary>
    /// مزود التخزين السحابي (S3, Cloudflare R2, Azure Blob, MinIO)
    /// </summary>
    public virtual string StorageProvider { get; set; } = string.Empty;

    /// <summary>
    /// اسم الحاوية أو الـ Bucket
    /// </summary>
    public virtual string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// المسار أو المفتاح الفريد داخل الـ Bucket
    /// </summary>
    public virtual string ObjectKey { get; set; } = string.Empty;

    /// <summary>
    /// المنطقة الجغرافية لخادم التخزين (Region)
    /// </summary>
    public virtual string? Region { get; set; }

    /// <summary>
    /// الرابط العام المباشر للوصول للملف
    /// </summary>
    public virtual string? PublicUrl { get; set; }

    /// <summary>
    /// رابط الصورة المصغرة (Thumbnail)
    /// </summary>
    public virtual string? ThumbnailUrl { get; set; }

    /// <summary>
    /// رابط الصورة بالحجم المتوسط
    /// </summary>
    public virtual string? MediumUrl { get; set; }

    /// <summary>
    /// رابط الصورة المحسنة والمضغوطة لسرعة التحميل (WebP)
    /// </summary>
    public virtual string? OptimizedUrl { get; set; }

    /// <summary>
    /// عرض الصورة بالبيكسل إن كانت صورة
    /// </summary>
    public virtual int? Width { get; set; }

    /// <summary>
    /// ارتفاع الصورة بالبيكسل إن كانت صورة
    /// </summary>
    public virtual int? Height { get; set; }

    /// <summary>
    /// البصمة الرقمية للملف (SHA256 Hash) لمنع تكرار الرفع
    /// </summary>
    public virtual string? Hash { get; set; }

    /// <summary>
    /// اسم نوع الكيان المرتبط بالملف (Product, Store, Receipt)
    /// </summary>
    public virtual string? EntityType { get; set; }

    /// <summary>
    /// معرف الكيان المرتبط
    /// </summary>
    public virtual Guid? EntityId { get; set; }

    /// <summary>
    /// هل الملف عام ومتاح بدون مصادقة؟
    /// </summary>
    public virtual bool IsPublic { get; set; } = true;

    /// <summary>
    /// هل الملف مشفر على الخادم؟
    /// </summary>
    public virtual bool IsEncrypted { get; set; }

    /// <summary>
    /// مستوى صلاحية الوصول للملف (عام، مصادقة، خاص)
    /// </summary>
    public virtual MediaAccessLevel AccessLevel { get; set; } = MediaAccessLevel.Public;

    /// <summary>
    /// حالة معالجة الملف (قيد الانتظار، تم الرفع، تمت المعالجة، فشل)
    /// </summary>
    public virtual MediaProcessingStatus Status { get; set; } = MediaProcessingStatus.Pending;

    /// <summary>
    /// تاريخ ووقت رفع الملف
    /// </summary>
    public virtual DateTime? UploadedAt { get; set; }

    /// <summary>
    /// تاريخ ووقت اكتمال المعالجة
    /// </summary>
    public virtual DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// معرف المستخدم الذي قام برفع الملف
    /// </summary>
    public virtual Guid? UploadedByUserId { get; set; }

    protected MediaFile()
    {
    }

    public MediaFile(
        Guid id,
        string fileName,
        string contentType,
        long fileSize,
        string storageProvider,
        string bucketName,
        string objectKey,
        string? publicUrl = null)
        : base(id)
    {
        FileName = fileName;
        ContentType = contentType;
        FileSize = fileSize;
        StorageProvider = storageProvider;
        BucketName = bucketName;
        ObjectKey = objectKey;
        PublicUrl = publicUrl;
        IsPublic = true;
        AccessLevel = MediaAccessLevel.Public;
        Status = MediaProcessingStatus.Uploaded;
        UploadedAt = DateTime.UtcNow;
    }
}
