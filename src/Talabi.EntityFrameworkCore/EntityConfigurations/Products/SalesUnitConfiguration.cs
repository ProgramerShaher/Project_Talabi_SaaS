using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.Products;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.Products;

/// <summary>
/// إعدادات جدول وحدات البيع الرئيسية في قاعدة البيانات
/// </summary>
public class SalesUnitConfiguration : IEntityTypeConfiguration<SalesUnit>
{
    public void Configure(EntityTypeBuilder<SalesUnit> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "SalesUnits", TalabiConsts.DbSchema);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(SalesUnitConsts.MaxNameLength);

        builder.Property(x => x.Code)
            .HasMaxLength(SalesUnitConsts.MaxCodeLength);

        builder.Property(x => x.Description)
            .HasMaxLength(SalesUnitConsts.MaxDescriptionLength);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.DisplayOrder)
            .HasDefaultValue(0);

        builder.HasIndex(x => new { x.TenantId, x.Name })
            .HasDatabaseName("IX_SalesUnits_TenantId_Name");
    }
}
