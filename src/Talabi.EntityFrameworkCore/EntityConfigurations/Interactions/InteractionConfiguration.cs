using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.Interactions;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.Interactions;

/// <summary>
/// إعدادات جدول التقييمات
/// </summary>
public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "Reviews", TalabiConsts.DbSchema, table =>
        {
            table.HasCheckConstraint("CK_Reviews_StoreRating", "[StoreRating] BETWEEN 1 AND 5");
        });

        builder.Property(x => x.Comment).HasMaxLength(ReviewConsts.MaxCommentLength);
        builder.Property(x => x.StoreReply).HasMaxLength(ReviewConsts.MaxStoreReplyLength);
        builder.Property(x => x.IsVisible).HasDefaultValue(true);

        // 1:1 فريد مع الطلب - معطل لأن التقييم للنشاط التجاري مباشرة وليس للطلب
        // builder.HasIndex(x => x.OrderId).IsUnique();

        // الفهارس المطلوبة
        builder.HasIndex(x => new { x.StoreId, x.IsVisible }).HasDatabaseName("IX_Reviews_StoreId_IsVisible");
        builder.HasIndex(x => x.CustomerId).HasDatabaseName("IX_Reviews_CustomerId");

        // builder.HasOne(x => x.Order)
        //     .WithMany()
        //     .HasForeignKey(x => x.OrderId)
        //     .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        // builder.HasOne(x => x.Courier)
        //     .WithMany()
        //     .HasForeignKey(x => x.CourierId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// إعدادات جدول المفضلة
/// </summary>
public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "Favorites", TalabiConsts.DbSchema);

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(FavoriteConsts.MaxEntityTypeLength);

        // الفهرس الفريد المركب: IX_Favorites_CustomerId_EntityType_EntityId
        builder.HasIndex(x => new { x.CustomerId, x.EntityType, x.EntityId })
            .IsUnique()
            .HasDatabaseName("IX_Favorites_CustomerId_EntityType_EntityId");

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
