using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.Products;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.Products;

/// <summary>
/// إعدادات جدول المنتجات في قاعدة البيانات
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "Products", TalabiConsts.DbSchema);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ProductConsts.MaxNameLength);

        builder.Property(x => x.Description)
            .HasMaxLength(ProductConsts.MaxDescriptionLength);

        builder.Property(x => x.ShortDescription)
            .HasMaxLength(ProductConsts.MaxShortDescriptionLength);

        builder.Property(x => x.SKU)
            .IsRequired()
            .HasMaxLength(ProductConsts.MaxSkuLength);

        // builder.Property(x => x.Barcode)
        //     .HasMaxLength(ProductConsts.MaxBarcodeLength);

        builder.Property(x => x.Unit)
            .IsRequired()
            .HasMaxLength(ProductConsts.MaxUnitLength);

        // builder.Property(x => x.Brand)
        //     .HasMaxLength(ProductConsts.MaxBrandLength);

        // builder.Property(x => x.WeightUnit)
        //     .HasMaxLength(ProductConsts.MaxWeightUnitLength);

        // builder.Property(x => x.Origin)
        //     .HasMaxLength(ProductConsts.MaxOriginLength);

        builder.Property(x => x.MainImageUrl)
            .HasMaxLength(ProductConsts.MaxMainImageUrlLength);

        builder.Property(x => x.Tags)
            .HasMaxLength(ProductConsts.MaxTagsLength);

        // الأسعار والأوزان
        builder.Property(x => x.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Discount).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(x => x.FinalPrice).HasPrecision(18, 2).IsRequired();
        // builder.Property(x => x.CostPrice).HasPrecision(18, 2);
        // builder.Property(x => x.Weight).HasPrecision(10, 2);

        builder.Property(x => x.MinOrderQuantity).HasDefaultValue(1);
        builder.Property(x => x.IsAvailable).HasDefaultValue(true);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsFeatured).HasDefaultValue(false);

        // الفهارس المطلوبة
        builder.HasIndex(x => new { x.StoreId, x.SKU })
            .IsUnique()
            .HasDatabaseName("IX_Products_StoreId_SKU");

        // builder.HasIndex(x => new { x.StoreId, x.Barcode })
        //     .IsUnique()
        //     .HasFilter("[Barcode] IS NOT NULL")
        //     .HasDatabaseName("IX_Products_StoreId_Barcode");

        builder.HasIndex(x => new { x.StoreId, x.IsActive })
            .HasDatabaseName("IX_Products_StoreId_IsActive");

        // العلاقات
        builder.HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StoreCategory)
            .WithMany()
            .HasForeignKey(x => x.StoreCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Images)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // builder.HasOne(x => x.Inventory)
        //     .WithOne(x => x.Product)
        //     .HasForeignKey<Inventory>(x => x.ProductId)
        //     .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// إعدادات جدول صور المنتجات
/// </summary>
public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "ProductImages", TalabiConsts.DbSchema);

        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(ProductImageConsts.MaxImageUrlLength);

        builder.Property(x => x.ThumbnailUrl)
            .HasMaxLength(ProductImageConsts.MaxThumbnailUrlLength);

        builder.Property(x => x.AltText)
            .HasMaxLength(ProductImageConsts.MaxAltTextLength);

        builder.Property(x => x.DisplayOrder).HasDefaultValue(0);
        builder.Property(x => x.IsPrimary).HasDefaultValue(false);

        // الفهرس: IX_ProductImages_ProductId_DisplayOrder
        builder.HasIndex(x => new { x.ProductId, x.DisplayOrder })
            .HasDatabaseName("IX_ProductImages_ProductId_DisplayOrder");
    }
}

// /// <summary>
// /// إعدادات جدول المخزون (معطل لأن النظام لا يتدخل في إدارة المخزون أو الكميات)
// /// </summary>
// public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
// {
//     public void Configure(EntityTypeBuilder<Inventory> builder)
//     {
//         builder.ConfigureByConvention();
// 
//         builder.ToTable(TalabiConsts.DbTablePrefix + "Inventory", TalabiConsts.DbSchema, table =>
//         {
//             // قيود التحقق في قاعدة البيانات
//             table.HasCheckConstraint("CK_Inventory_CurrentQuantity", "[CurrentQuantity] >= 0");
//             table.HasCheckConstraint("CK_Inventory_ReservedQuantity", "[ReservedQuantity] >= 0");
//         });
// 
//         builder.Property(x => x.WarehouseLocation)
//             .HasMaxLength(InventoryConsts.MaxWarehouseLocationLength);
// 
//         builder.Property(x => x.CurrentQuantity).HasDefaultValue(0);
//         builder.Property(x => x.ReservedQuantity).HasDefaultValue(0);
//         builder.Property(x => x.MinStockLevel).HasDefaultValue(InventoryConsts.DefaultMinStockLevel);
//         builder.Property(x => x.MaxStockLevel).HasDefaultValue(InventoryConsts.DefaultMaxStockLevel);
//         builder.Property(x => x.ReorderLevel).HasDefaultValue(InventoryConsts.DefaultReorderLevel);
//         builder.Property(x => x.IsTracked).HasDefaultValue(true);
// 
//         // الفهرس الفريد المركب: IX_Inventory_StoreId_ProductId
//         builder.HasIndex(x => new { x.StoreId, x.ProductId })
//             .IsUnique()
//             .HasDatabaseName("IX_Inventory_StoreId_ProductId");
// 
//         builder.HasOne(x => x.Store)
//             .WithMany()
//             .HasForeignKey(x => x.StoreId)
//             .OnDelete(DeleteBehavior.Restrict);
//     }
// }
