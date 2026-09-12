using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Talabi.MediaFiles.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Talabi.MediaFiles;

/// <summary>
/// تنفيذ خدمة إدارة וرفع الملفات (تعمل حالياً بالتخزين المحلي)
/// </summary>
[Authorize] // يحتاج لتسجيل دخول لرفع الملفات
public class MediaFileAppService : ApplicationService, IMediaFileAppService
{
    private readonly IRepository<MediaFile, Guid> _mediaFileRepository;
    private readonly IConfiguration _configuration;
    private readonly string _uploadPath;
    private readonly long _maxFileSizeBytes;
    private readonly string[] _allowedExtensions;

    public MediaFileAppService(
        IRepository<MediaFile, Guid> mediaFileRepository,
        IConfiguration configuration)
    {
        _mediaFileRepository = mediaFileRepository;
        _configuration = configuration;

        // قراءة الإعدادات من appsettings.json
        _uploadPath = _configuration["MediaStorage:LocalUploadPath"] ?? "wwwroot/uploads/media";
        var maxFileSizeMb = int.Parse(_configuration["MediaStorage:MaxFileSizeMb"] ?? "10");
        _maxFileSizeBytes = maxFileSizeMb * 1024 * 1024;
        
        var extensionsStr = _configuration["MediaStorage:AllowedExtensions"] ?? ".jpg,.jpeg,.png,.gif,.webp,.pdf,.doc,.docx,.xls,.xlsx";
        _allowedExtensions = extensionsStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim().ToLowerInvariant()).ToArray();
    }

    public async Task<MediaFileDto> UploadAsync(UploadMediaFileInput input)
    {
        if (input.File == null)
        {
            throw new UserFriendlyException("لم يتم توفير أي ملف للرفع.");
        }

        var fileName = input.File.FileName ?? "unknown_file";
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        // التحقق من الحجم
        if (input.File.ContentLength > _maxFileSizeBytes)
        {
            throw new UserFriendlyException($"حجم الملف يتجاوز الحد الأقصى المسموح به وهو {_maxFileSizeBytes / 1024 / 1024} ميجابايت.");
        }

        // التحقق من الامتداد
        if (!string.IsNullOrEmpty(extension) && !_allowedExtensions.Contains(extension))
        {
            throw new UserFriendlyException($"نوع الملف غير مدعوم. الأنواع المدعومة: {string.Join(", ", _allowedExtensions)}");
        }

        // إنشاء المجلد إذا لم يكن موجوداً
        var targetDirectory = Path.Combine(Directory.GetCurrentDirectory(), _uploadPath);
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        // قراءة الملف وحساب Hash
        using var stream = input.File.GetStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        var hash = CalculateHash(fileBytes);

        // التحقق مما إذا كان الملف موجوداً مسبقاً (منع التكرار)
        var existingFile = await _mediaFileRepository.FirstOrDefaultAsync(x => x.Hash == hash);
        if (existingFile != null)
        {
            Logger.LogInformation("تم رفع الملف مسبقاً. إعادة استخدام الرابط: {Url}", existingFile.PublicUrl);
            return MapToDto(existingFile);
        }

        // توليد اسم ملف فريد
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(targetDirectory, uniqueFileName);

        // حفظ الملف على القرص
        await File.WriteAllBytesAsync(physicalPath, fileBytes);

        // بناء الرابط العام
        // إزالة 'wwwroot' من المسار إذا كانت موجودة لإنشاء رابط URL صحيح
        var urlPath = _uploadPath.StartsWith("wwwroot/") || _uploadPath.StartsWith("wwwroot\\") 
            ? _uploadPath.Substring(8) 
            : _uploadPath;
            
        var publicUrl = $"/{urlPath.Replace('\\', '/')}/{uniqueFileName}";

        // إنشاء السجل في قاعدة البيانات
        var mediaFile = new MediaFile(
            id: GuidGenerator.Create(),
            fileName: uniqueFileName,
            contentType: input.File.ContentType ?? "application/octet-stream",
            fileSize: fileBytes.Length,
            storageProvider: "LocalFileSystem",
            bucketName: "local-uploads",
            objectKey: physicalPath,
            publicUrl: publicUrl
        )
        {
            OriginalFileName = fileName,
            Extension = extension,
            Hash = hash,
            EntityType = input.EntityType,
            EntityId = input.EntityId,
            IsPublic = input.IsPublic
        };

        await _mediaFileRepository.InsertAsync(mediaFile, autoSave: true);

        Logger.LogInformation("تم رفع ملف جديد بنجاح وحفظه في: {Path}", physicalPath);

        return MapToDto(mediaFile);
    }

    public async Task<MediaFileDto> GetAsync(Guid id)
    {
        var mediaFile = await _mediaFileRepository.GetAsync(id);
        return MapToDto(mediaFile);
    }

    public async Task DeleteAsync(Guid id)
    {
        var mediaFile = await _mediaFileRepository.GetAsync(id);
        
        // حذف الملف الفعلي من القرص
        if (mediaFile.StorageProvider == "LocalFileSystem" && File.Exists(mediaFile.ObjectKey))
        {
            try
            {
                File.Delete(mediaFile.ObjectKey);
                Logger.LogInformation("تم حذف الملف من القرص: {Path}", mediaFile.ObjectKey);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "فشل في حذف الملف من القرص: {Path}", mediaFile.ObjectKey);
            }
        }

        await _mediaFileRepository.DeleteAsync(id);
    }

    private static string CalculateHash(byte[] content)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(content);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private static MediaFileDto MapToDto(MediaFile entity)
    {
        return new MediaFileDto
        {
            Id = entity.Id,
            FileName = entity.FileName,
            OriginalFileName = entity.OriginalFileName,
            ContentType = entity.ContentType,
            FileSize = entity.FileSize,
            Extension = entity.Extension,
            StorageProvider = entity.StorageProvider,
            PublicUrl = entity.PublicUrl,
            ThumbnailUrl = entity.ThumbnailUrl,
            MediumUrl = entity.MediumUrl,
            OptimizedUrl = entity.OptimizedUrl,
            Width = entity.Width,
            Height = entity.Height,
            EntityType = entity.EntityType,
            EntityId = entity.EntityId,
            IsPublic = entity.IsPublic,
            AccessLevel = entity.AccessLevel,
            Status = entity.Status,
            UploadedAt = entity.UploadedAt
        };
    }
}
