using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.Notifications;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.Notifications;

/// <summary>
/// إعدادات جدول أنواع الإشعارات
/// </summary>
public class NotificationTypeConfiguration : IEntityTypeConfiguration<NotificationType>
{
    public void Configure(EntityTypeBuilder<NotificationType> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "NotificationTypes", TalabiConsts.DbSchema);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(NotificationTypeConsts.MaxNameLength);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(NotificationTypeConsts.MaxDisplayNameLength);

        builder.Property(x => x.Icon).HasMaxLength(NotificationTypeConsts.MaxIconLength);
        builder.Property(x => x.Color).HasMaxLength(NotificationTypeConsts.MaxColorLength);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}

/// <summary>
/// إعدادات جدول الإشعارات
/// </summary>
public class AppNotificationConfiguration : IEntityTypeConfiguration<AppNotification>
{
    public void Configure(EntityTypeBuilder<AppNotification> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "Notifications", TalabiConsts.DbSchema);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(AppNotificationConsts.MaxTitleLength);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(AppNotificationConsts.MaxMessageLength);

        builder.Property(x => x.RelatedEntityName).HasMaxLength(AppNotificationConsts.MaxRelatedEntityNameLength);
        builder.Property(x => x.ActionUrl).HasMaxLength(AppNotificationConsts.MaxActionUrlLength);
        builder.Property(x => x.SentVia).HasMaxLength(AppNotificationConsts.MaxSentViaLength);
        builder.Property(x => x.IsRead).HasDefaultValue(false);

        // الفهرس: IX_Notifications_RecipientUserId_IsRead_CreatedAt
        builder.HasIndex(x => new { x.RecipientUserId, x.IsRead, x.CreationTime })
            .HasDatabaseName("IX_Notifications_RecipientUserId_IsRead_CreatedAt");

        builder.HasOne(x => x.NotificationType)
            .WithMany()
            .HasForeignKey(x => x.NotificationTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
