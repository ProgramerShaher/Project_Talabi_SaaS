using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.Customers;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.Customers;

/// <summary>
/// إعدادات جدول العملاء في قاعدة البيانات
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "Customers", TalabiConsts.DbSchema);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.PreferredLanguage)
            .IsRequired()
            .HasMaxLength(CustomerConsts.MaxPreferredLanguageLength)
            .HasDefaultValue(CustomerConsts.DefaultPreferredLanguage);

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(CustomerConsts.MaxAvatarUrlLength);

        builder.Property(x => x.LoyaltyPoints)
            .HasDefaultValue(0);

        // فهرس فريد على UserId لضمان علاقة 1:1 مع جدول المستخدمين
        builder.HasIndex(x => x.UserId).IsUnique();

        // علاقة 1:N مع عناوين العميل
        builder.HasMany(x => x.Addresses)
            .WithOne(x => x.Customer)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// إعدادات جدول عناوين العملاء في قاعدة البيانات
/// </summary>
public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "CustomerAddresses", TalabiConsts.DbSchema);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(CustomerAddressConsts.MaxTitleLength);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(CustomerAddressConsts.MaxCityLength);

        builder.Property(x => x.District)
            .IsRequired()
            .HasMaxLength(CustomerAddressConsts.MaxDistrictLength);

        builder.Property(x => x.Street)
            .IsRequired()
            .HasMaxLength(CustomerAddressConsts.MaxStreetLength);

        builder.Property(x => x.Building)
            .HasMaxLength(CustomerAddressConsts.MaxBuildingLength);

        builder.Property(x => x.Floor)
            .HasMaxLength(CustomerAddressConsts.MaxFloorLength);

        builder.Property(x => x.Apartment)
            .HasMaxLength(CustomerAddressConsts.MaxApartmentLength);

        builder.Property(x => x.AdditionalDetails)
            .HasMaxLength(CustomerAddressConsts.MaxAdditionalDetailsLength);

        // إحداثيات الموقع بدقة decimal(9,6)
        builder.Property(x => x.Latitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.Longitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .HasDefaultValue(false);

        // الفهرس المطلوب: IX_CustomerAddresses_CustomerId_IsDefault
        builder.HasIndex(x => new { x.CustomerId, x.IsDefault })
            .HasDatabaseName("IX_CustomerAddresses_CustomerId_IsDefault");
    }
}
