using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.Carts;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.Carts;

/// <summary>
/// إعدادات جدول سلات الشراء في قاعدة البيانات
/// </summary>
public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "Carts", TalabiConsts.DbSchema);

        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.LastActivityAt).IsRequired();

        // فهرس فريد جزئي للسلة النشطة لنفس العميل مع نفس المتجر
        builder.HasIndex(x => new { x.CustomerId, x.StoreId, x.IsActive })
            .HasFilter("[IsActive] = 1")
            .IsUnique()
            .HasDatabaseName("IX_Carts_CustomerId_StoreId_IsActive");

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Cart)
            .HasForeignKey(x => x.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// إعدادات جدول عناصر السلة
/// </summary>
public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ConfigureByConvention();

        builder.ToTable(TalabiConsts.DbTablePrefix + "CartItems", TalabiConsts.DbSchema, table =>
        {
            table.HasCheckConstraint("CK_CartItems_Quantity", "[Quantity] > 0");
        });

        builder.Property(x => x.UnitPriceAtAddition).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(CartConsts.MaxNotesLength);
        builder.Property(x => x.AddedAt).IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
