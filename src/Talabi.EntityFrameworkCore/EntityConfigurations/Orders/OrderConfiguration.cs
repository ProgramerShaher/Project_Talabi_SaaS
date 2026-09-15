using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.Orders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.Orders;

/// <summary>
/// إعدادات جدول حالات الطلب
/// </summary>
public class OrderStatusConfiguration : IEntityTypeConfiguration<OrderStatus>
{
    public void Configure(EntityTypeBuilder<OrderStatus> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "OrderStatuses", TalabiConsts.DbSchema);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(OrderStatusConsts.MaxNameLength);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(OrderStatusConsts.MaxDisplayNameLength);

        builder.Property(x => x.Color).HasMaxLength(OrderStatusConsts.MaxColorLength);
        builder.Property(x => x.Icon).HasMaxLength(OrderStatusConsts.MaxIconLength);
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0);
        builder.Property(x => x.IsFinal).HasDefaultValue(false);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}

/// <summary>
/// إعدادات جدول الطلبات الرئيسي
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "Orders", TalabiConsts.DbSchema);

        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(OrderConsts.MaxOrderNumberLength);

        builder.Property(x => x.DeliveryAddressSnapshot).IsRequired();
        builder.Property(x => x.CustomerNotes).HasMaxLength(OrderConsts.MaxCustomerNotesLength);
        builder.Property(x => x.StoreNotes).HasMaxLength(OrderConsts.MaxStoreNotesLength);

        // المبالغ المالية
        builder.Property(x => x.SubTotal).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0);
        // builder.Property(x => x.DeliveryFee).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(x => x.FinalAmount).HasPrecision(18, 2).HasDefaultValue(0);

        builder.Property(x => x.PaymentStatus).HasDefaultValue(OrderPaymentStatus.Pending);
        builder.Property(x => x.PaymentReceiptUrl).HasMaxLength(2048);

        // الفهارس المطلوبة
        builder.HasIndex(x => x.OrderNumber).IsUnique().HasDatabaseName("IX_Orders_OrderNumber");
        builder.HasIndex(x => new { x.CustomerId, x.CreationTime }).HasDatabaseName("IX_Orders_CustomerId_CreatedAt");
        builder.HasIndex(x => new { x.StoreId, x.OrderStatusId }).HasDatabaseName("IX_Orders_StoreId_OrderStatusId");

        // العلاقات
        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OrderStatus)
            .WithMany()
            .HasForeignKey(x => x.OrderStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StatusHistories)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Rejection)
            .WithOne(x => x.Order)
            .HasForeignKey<OrderRejection>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Cancellation)
            .WithOne(x => x.Order)
            .HasForeignKey<OrderCancellation>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// إعدادات جدول عناصر وتفاصيل الطلب
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "OrderItems", TalabiConsts.DbSchema, table =>
        {
            table.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] > 0");
        });

        builder.Property(x => x.ProductName)
            .IsRequired()
            .HasMaxLength(OrderItemConsts.MaxProductNameLength);

        builder.Property(x => x.ProductImageUrl).HasMaxLength(OrderItemConsts.MaxProductImageUrlLength);
        builder.Property(x => x.SKU).IsRequired().HasMaxLength(OrderItemConsts.MaxSkuLength);
        builder.Property(x => x.Unit).IsRequired().HasMaxLength(OrderItemConsts.MaxUnitLength);
        builder.Property(x => x.Notes).HasMaxLength(OrderItemConsts.MaxNotesLength);

        builder.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Discount).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(x => x.TotalPrice).HasPrecision(18, 2).IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// إعدادات جدول سجل حالات الطلب
/// </summary>
public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "OrderStatusHistories", TalabiConsts.DbSchema);

        builder.Property(x => x.ChangedByRole)
            .IsRequired()
            .HasMaxLength(OrderStatusHistoryConsts.MaxChangedByRoleLength);

        builder.Property(x => x.Notes).HasMaxLength(OrderStatusHistoryConsts.MaxNotesLength);

        builder.HasIndex(x => new { x.OrderId, x.CreationTime })
            .HasDatabaseName("IX_OrderStatusHistories_OrderId_CreatedAt");

        builder.HasOne(x => x.FromStatus)
            .WithMany()
            .HasForeignKey(x => x.FromStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToStatus)
            .WithMany()
            .HasForeignKey(x => x.ToStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// إعدادات جدول أسباب الإلغاء والرفض
/// </summary>
public class CancellationReasonConfiguration : IEntityTypeConfiguration<CancellationReason>
{
    public void Configure(EntityTypeBuilder<CancellationReason> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "CancellationReasons", TalabiConsts.DbSchema);

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(CancellationReasonConsts.MaxReasonLength);

        builder.Property(x => x.IsActive).HasDefaultValue(true);
    }
}

/// <summary>
/// إعدادات جدول رفض الطلبات
/// </summary>
public class OrderRejectionConfiguration : IEntityTypeConfiguration<OrderRejection>
{
    public void Configure(EntityTypeBuilder<OrderRejection> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "OrderRejections", TalabiConsts.DbSchema);

        builder.Property(x => x.AdditionalNotes).HasMaxLength(CancellationReasonConsts.MaxAdditionalNotesLength);

        builder.HasIndex(x => x.OrderId).IsUnique();

        builder.HasOne(x => x.RejectionReason)
            .WithMany()
            .HasForeignKey(x => x.RejectionReasonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// إعدادات جدول إلغاء الطلبات
/// </summary>
public class OrderCancellationConfiguration : IEntityTypeConfiguration<OrderCancellation>
{
    public void Configure(EntityTypeBuilder<OrderCancellation> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "OrderCancellations", TalabiConsts.DbSchema);

        builder.Property(x => x.CancelledByRole)
            .IsRequired()
            .HasMaxLength(CancellationReasonConsts.MaxCancelledByRoleLength);

        builder.Property(x => x.AdditionalNotes).HasMaxLength(CancellationReasonConsts.MaxAdditionalNotesLength);

        builder.HasIndex(x => x.OrderId).IsUnique();

        builder.HasOne(x => x.CancellationReason)
            .WithMany()
            .HasForeignKey(x => x.CancellationReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PreviousStatus)
            .WithMany()
            .HasForeignKey(x => x.PreviousStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
