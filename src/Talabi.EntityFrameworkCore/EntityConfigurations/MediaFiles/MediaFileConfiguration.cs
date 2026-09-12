using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.MediaFiles;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.MediaFiles;

/// <summary>
/// إعدادات جدول الملفات والوسائط الرقمية
/// </summary>
public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "MediaFiles", TalabiConsts.DbSchema);

        builder.Property(x => x.FileName).IsRequired().HasMaxLength(MediaFileConsts.MaxFileNameLength);
        builder.Property(x => x.OriginalFileName).HasMaxLength(MediaFileConsts.MaxOriginalFileNameLength);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(MediaFileConsts.MaxContentTypeLength);
        builder.Property(x => x.Extension).HasMaxLength(MediaFileConsts.MaxExtensionLength);
        builder.Property(x => x.StorageProvider).IsRequired().HasMaxLength(MediaFileConsts.MaxStorageProviderLength);
        builder.Property(x => x.BucketName).IsRequired().HasMaxLength(MediaFileConsts.MaxBucketNameLength);
        builder.Property(x => x.ObjectKey).IsRequired().HasMaxLength(MediaFileConsts.MaxObjectKeyLength);
        builder.Property(x => x.Region).HasMaxLength(MediaFileConsts.MaxRegionLength);

        builder.Property(x => x.PublicUrl).HasMaxLength(MediaFileConsts.MaxUrlLength);
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(MediaFileConsts.MaxUrlLength);
        builder.Property(x => x.MediumUrl).HasMaxLength(MediaFileConsts.MaxUrlLength);
        builder.Property(x => x.OptimizedUrl).HasMaxLength(MediaFileConsts.MaxUrlLength);

        builder.Property(x => x.Hash).HasMaxLength(MediaFileConsts.MaxHashLength);
        builder.Property(x => x.EntityType).HasMaxLength(MediaFileConsts.MaxEntityTypeLength);

        builder.Property(x => x.IsPublic).HasDefaultValue(true);
        builder.Property(x => x.IsEncrypted).HasDefaultValue(false);
        builder.Property(x => x.AccessLevel).HasDefaultValue(MediaAccessLevel.Public);
        builder.Property(x => x.Status).HasDefaultValue(MediaProcessingStatus.Uploaded);

        // الفهارس المطلوبة
        builder.HasIndex(x => new { x.EntityType, x.EntityId })
            .HasDatabaseName("IX_MediaFiles_EntityType_EntityId");

        builder.HasIndex(x => x.Hash)
            .HasDatabaseName("IX_MediaFiles_Hash");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_MediaFiles_Status");
    }
}
