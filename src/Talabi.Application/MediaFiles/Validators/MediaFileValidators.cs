using System;
using System.IO;
using System.Linq;
using FluentValidation;
using Talabi.MediaFiles;
using Talabi.MediaFiles.Dtos;

namespace Talabi.MediaFiles.Validators;

#region Media File Validators
/// <summary>
/// محدد قواعد التحقق الصارمة لرفع الملفات والوسائط
/// </summary>
public class UploadMediaFileInputValidator : AbstractValidator<UploadMediaFileInput>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".pdf", ".svg" };
    private const long MaxFileSizeInBytes = 15 * 1024 * 1024; // 15MB

    public UploadMediaFileInputValidator()
    {
        #region Rules
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("الملف المرفوع مطلوب ولا يمكن أن يكون فارغاً.")
            .Must(file => file != null && !string.IsNullOrWhiteSpace(file.FileName))
            .WithMessage("اسم الملف المرفوع غير صالح.")
            .Must(file =>
            {
                if (file == null || string.IsNullOrEmpty(file.FileName)) return false;
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                return AllowedExtensions.Contains(ext);
            })
            .WithMessage($"نوع الملف غير مسموح به. الأنواع المدعومة فقط: {string.Join(", ", AllowedExtensions)}.")
            .Must(file =>
            {
                if (file?.ContentLength != null)
                {
                    return file.ContentLength.Value <= MaxFileSizeInBytes;
                }
                return true;
            })
            .WithMessage("حجم الملف المرفوع يتجاوز الحد الأقصى المسموح به وهو 15 ميجابايت.");

        When(x => !string.IsNullOrEmpty(x.EntityType), () =>
        {
            RuleFor(x => x.EntityType)
                .MaximumLength(MediaFileConsts.MaxEntityTypeLength)
                .WithMessage($"نوع الكيان المرتبط بالملف لا يتجاوز {MediaFileConsts.MaxEntityTypeLength} حرفاً.");
        });
        #endregion
    }
}
#endregion
