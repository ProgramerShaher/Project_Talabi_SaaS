using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.Deliveries;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.Deliveries;

/// <summary>
/// إعدادات جدول مناديب التوصيل
/// </summary>
public class CourierConfiguration : IEntityTypeConfiguration<Courier>
{
    public void Configure(EntityTypeBuilder<Courier> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "Couriers", TalabiConsts.DbSchema);

        builder.Property(x => x.VehicleType).HasMaxLength(CourierConsts.MaxVehicleTypeLength);
        builder.Property(x => x.VehicleNumber).HasMaxLength(CourierConsts.MaxVehicleNumberLength);
        builder.Property(x => x.LicenseNumber).HasMaxLength(CourierConsts.MaxLicenseNumberLength);

        builder.Property(x => x.CurrentLatitude).HasPrecision(9, 6);
        builder.Property(x => x.CurrentLongitude).HasPrecision(9, 6);
        // builder.Property(x => x.Rating).HasPrecision(3, 2).HasDefaultValue(0);
        builder.Property(x => x.TotalDeliveries).HasDefaultValue(0);
        // builder.Property(x => x.TotalEarnings).HasPrecision(18, 2).HasDefaultValue(0);

        builder.Property(x => x.IsAvailable).HasDefaultValue(true);
        builder.Property(x => x.IsOnline).HasDefaultValue(false);

        builder.HasIndex(x => x.UserId).IsUnique();
    }
}

/// <summary>
/// إعدادات جدول تعيينات التوصيل
/// </summary>
public class DeliveryAssignmentConfiguration : IEntityTypeConfiguration<DeliveryAssignment>
{
    public void Configure(EntityTypeBuilder<DeliveryAssignment> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "DeliveryAssignments", TalabiConsts.DbSchema);

        builder.Property(x => x.DistanceKm).HasPrecision(6, 2);
        // builder.Property(x => x.DeliveryFee).HasPrecision(18, 2);
        // builder.Property(x => x.CourierEarning).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasDefaultValue(DeliveryAssignmentStatus.Assigned);

        // 1:1 فريد مع الطلب
        builder.HasIndex(x => x.OrderId).IsUnique();

        // الفهرس: IX_DeliveryAssignments_CourierId_Status
        builder.HasIndex(x => new { x.CourierId, x.Status })
            .HasDatabaseName("IX_DeliveryAssignments_CourierId_Status");

        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Courier)
            .WithMany(x => x.Assignments)
            .HasForeignKey(x => x.CourierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Confirmation)
            .WithOne(x => x.DeliveryAssignment)
            .HasForeignKey<DeliveryConfirmation>(x => x.DeliveryAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// إعدادات جدول تأكيدات التسليم
/// </summary>
public class DeliveryConfirmationConfiguration : IEntityTypeConfiguration<DeliveryConfirmation>
{
    public void Configure(EntityTypeBuilder<DeliveryConfirmation> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "DeliveryConfirmations", TalabiConsts.DbSchema);

        builder.Property(x => x.VerificationCode).HasMaxLength(DeliveryConfirmationConsts.MaxVerificationCodeLength);
        builder.Property(x => x.SignatureUrl).HasMaxLength(DeliveryConfirmationConsts.MaxSignatureUrlLength);
        builder.Property(x => x.PhotoProofUrl).HasMaxLength(DeliveryConfirmationConsts.MaxPhotoProofUrlLength);
        builder.Property(x => x.Notes).HasMaxLength(DeliveryConfirmationConsts.MaxNotesLength);

        builder.HasIndex(x => x.DeliveryAssignmentId).IsUnique();
    }
}
