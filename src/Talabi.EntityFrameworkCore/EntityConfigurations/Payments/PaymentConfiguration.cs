using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.Payments;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.Payments;

/// <summary>
/// إعدادات جدول طرق الدفع
/// </summary>
public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "PaymentMethods", TalabiConsts.DbSchema);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(PaymentMethodConsts.MaxNameLength);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(PaymentMethodConsts.MaxDisplayNameLength);

        builder.Property(x => x.IconUrl).HasMaxLength(PaymentMethodConsts.MaxIconUrlLength);
        builder.Property(x => x.IsOnline).HasDefaultValue(false);
        builder.Property(x => x.RequiresReceipt).HasDefaultValue(false);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}

/// <summary>
/// إعدادات جدول عمليات الدفع
/// </summary>
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "Payments", TalabiConsts.DbSchema);

        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.RefundAmount).HasPrecision(18, 2);
        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(PaymentConsts.MaxCurrencyLength)
            .HasDefaultValue(PaymentConsts.DefaultCurrency);

        builder.Property(x => x.TransactionReference).HasMaxLength(PaymentConsts.MaxTransactionReferenceLength);
        builder.Property(x => x.Status).HasDefaultValue(PaymentTransactionStatus.Pending);

        // علاقة 1:1 فريدة مع الطلب
        builder.HasIndex(x => x.OrderId).IsUnique();

        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaymentMethod)
            .WithMany()
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Receipt)
            .WithOne(x => x.Payment)
            .HasForeignKey<PaymentReceipt>(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// إعدادات جدول إيصالات الدفع
/// </summary>
public class PaymentReceiptConfiguration : IEntityTypeConfiguration<PaymentReceipt>
{
    public void Configure(EntityTypeBuilder<PaymentReceipt> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "PaymentReceipts", TalabiConsts.DbSchema);

        builder.Property(x => x.WalletName).HasMaxLength(PaymentReceiptConsts.MaxWalletNameLength);
        builder.Property(x => x.TransactionNumber).HasMaxLength(PaymentReceiptConsts.MaxTransactionNumberLength);
        builder.Property(x => x.RejectionReason).HasMaxLength(PaymentReceiptConsts.MaxRejectionReasonLength);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.VerificationStatus).HasDefaultValue(ReceiptVerificationStatus.Pending);

        builder.HasIndex(x => x.PaymentId).IsUnique();
    }
}
