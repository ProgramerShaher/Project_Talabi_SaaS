using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.Categories;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.Categories;

/// <summary>
/// إعدادات جدول التصنيفات العامة في قاعدة البيانات
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "Categories", TalabiConsts.DbSchema);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(CategoryConsts.MaxNameLength);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(CategoryConsts.MaxSlugLength);

        builder.Property(x => x.Description)
            .HasMaxLength(CategoryConsts.MaxDescriptionLength);

        builder.Property(x => x.IconUrl)
            .HasMaxLength(CategoryConsts.MaxIconUrlLength);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(CategoryConsts.MaxImageUrlLength);

        builder.Property(x => x.Path)
            .HasMaxLength(CategoryConsts.MaxPathLength);

        builder.Property(x => x.IsGlobal).HasDefaultValue(true);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);
        builder.Property(x => x.Level).HasDefaultValue(0);

        // الفهارس
        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("IX_Categories_Slug");
        builder.HasIndex(x => x.ParentId).HasDatabaseName("IX_Categories_ParentId");
        builder.HasIndex(x => new { x.IsGlobal, x.IsActive }).HasDatabaseName("IX_Categories_IsGlobal_IsActive");

        // علاقة شجرية ذاتية
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// إعدادات جدول تصنيفات المتاجر في قاعدة البيانات
/// </summary>
public class StoreCategoryConfiguration : IEntityTypeConfiguration<StoreCategory>
{
    public void Configure(EntityTypeBuilder<StoreCategory> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "StoreCategories", TalabiConsts.DbSchema);

        builder.Property(x => x.CustomName)
            .HasMaxLength(StoreCategoryConsts.MaxCustomNameLength);

        builder.Property(x => x.CustomIconUrl)
            .HasMaxLength(StoreCategoryConsts.MaxCustomIconUrlLength);

        builder.Property(x => x.Description)
            .HasMaxLength(StoreCategoryConsts.MaxDescriptionLength);

        builder.Property(x => x.IsHidden).HasDefaultValue(false);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);

        // الفهرس المطلوب: IX_StoreCategories_StoreId_ParentId
        builder.HasIndex(x => new { x.StoreId, x.ParentId }).HasDatabaseName("IX_StoreCategories_StoreId_ParentId");

        // العلاقات
        builder.HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.GlobalCategory)
            .WithMany()
            .HasForeignKey(x => x.GlobalCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
