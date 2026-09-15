using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.Stores;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.Stores;

/// <summary>
/// إعدادات جدول أنواع المتاجر في قاعدة البيانات
/// </summary>
public class StoreTypeConfiguration : IEntityTypeConfiguration<StoreType>
{
    public void Configure(EntityTypeBuilder<StoreType> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "StoreTypes", TalabiConsts.DbSchema);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(StoreTypeConsts.MaxNameLength);

        builder.Property(x => x.IconUrl)
            .HasMaxLength(StoreTypeConsts.MaxIconUrlLength);

        builder.Property(x => x.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}

/// <summary>
/// إعدادات جدول المتاجر في قاعدة البيانات
/// </summary>
public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "Stores", TalabiConsts.DbSchema);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(StoreConsts.MaxNameLength);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(StoreConsts.MaxSlugLength);

        builder.Property(x => x.Description)
            .HasMaxLength(StoreConsts.MaxDescriptionLength);

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(StoreConsts.MaxLogoUrlLength);

        builder.Property(x => x.CoverImageUrl)
            .HasMaxLength(StoreConsts.MaxCoverImageUrlLength);

        builder.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(StoreConsts.MaxPhoneLength);

        builder.Property(x => x.Email)
            .HasMaxLength(StoreConsts.MaxEmailLength);

        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(StoreConsts.MaxAddressLength);

        // الإحداثيات
        builder.Property(x => x.Latitude).HasPrecision(9, 6);
        builder.Property(x => x.Longitude).HasPrecision(9, 6);

        // المبالغ والتقييم
        builder.Property(x => x.MinimumOrderAmount).HasPrecision(18, 2).HasDefaultValue(0);
        // builder.Property(x => x.DeliveryFee).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(x => x.Rating).HasPrecision(3, 2).HasDefaultValue(0);

        builder.Property(x => x.Status).HasDefaultValue(StoreStatus.PendingApproval);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsFeatured).HasDefaultValue(false);
        builder.Property(x => x.TotalReviews).HasDefaultValue(0);
        builder.Property(x => x.TotalOrders).HasDefaultValue(0);

        // الفهارس المطلوبة
        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("IX_Stores_Slug");
        builder.HasIndex(x => new { x.StoreTypeId, x.IsActive }).HasDatabaseName("IX_Stores_StoreTypeId_IsActive");
        builder.HasIndex(x => x.OwnerId).HasDatabaseName("IX_Stores_OwnerId");

        // العلاقات
        builder.HasOne(x => x.StoreType)
            .WithMany(x => x.Stores)
            .HasForeignKey(x => x.StoreTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.StoreUsers)
            .WithOne(x => x.Store)
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PaymentAccounts)
            .WithOne(x => x.Store)
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// إعدادات جدول موظفي ومسؤولي المتاجر
/// </summary>
public class StoreUserConfiguration : IEntityTypeConfiguration<StoreUser>
{
    public void Configure(EntityTypeBuilder<StoreUser> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "StoreUsers", TalabiConsts.DbSchema);

        builder.Property(x => x.Role).IsRequired();
        builder.Property(x => x.JoinedAt).IsRequired();

        // الفهرس الفريد المركب: IX_StoreUsers_StoreId_UserId
        builder.HasIndex(x => new { x.StoreId, x.UserId })
            .IsUnique()
            .HasDatabaseName("IX_StoreUsers_StoreId_UserId");
    }
}

/// <summary>
/// إعدادات جدول حسابات الدفع للمتاجر
/// </summary>
public class StorePaymentAccountConfiguration : IEntityTypeConfiguration<StorePaymentAccount>
{
    public void Configure(EntityTypeBuilder<StorePaymentAccount> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "StorePaymentAccounts", TalabiConsts.DbSchema);

        builder.Property(x => x.ProviderName).IsRequired().HasMaxLength(64);
        builder.Property(x => x.AccountNumber).IsRequired().HasMaxLength(64);
        builder.Property(x => x.AccountName).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Notes).HasMaxLength(512);
        
        builder.HasIndex(x => x.StoreId);
    }
}
