using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.Products;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.Products;

/// <summary>
/// إعدادات جدول ربط وحدات بيع المنتجات في قاعدة البيانات
/// </summary>
public class ProductSalesUnitConfiguration : IEntityTypeConfiguration<ProductSalesUnit>
{
    public void Configure(EntityTypeBuilder<ProductSalesUnit> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "ProductSalesUnits", TalabiConsts.DbSchema);

        builder.Property(x => x.UnitName)
            .IsRequired()
            .HasMaxLength(SalesUnitConsts.MaxNameLength);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Property(x => x.IsDefault)
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.DisplayOrder)
            .HasDefaultValue(0);

        // العلاقات
        builder.HasOne(x => x.Product)
            .WithMany(x => x.SalesUnits)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SalesUnit)
            .WithMany()
            .HasForeignKey(x => x.SalesUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // الفهارس
        builder.HasIndex(x => new { x.ProductId, x.SalesUnitId })
            .IsUnique()
            .HasDatabaseName("IX_ProductSalesUnits_ProductId_SalesUnitId");

        builder.HasIndex(x => new { x.ProductId, x.IsDefault })
            .HasDatabaseName("IX_ProductSalesUnits_ProductId_IsDefault");
    }
}
