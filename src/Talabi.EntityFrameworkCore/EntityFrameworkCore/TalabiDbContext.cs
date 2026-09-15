using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Talabi.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class TalabiDbContext :
    AbpDbContext<TalabiDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    #region Talabi Core SaaS Entities

    // 1. Customers
    public DbSet<Talabi.Customers.Customer> Customers { get; set; }
    public DbSet<Talabi.Customers.CustomerAddress> CustomerAddresses { get; set; }

    // 2. Stores
    public DbSet<Talabi.Stores.StoreType> StoreTypes { get; set; }
    public DbSet<Talabi.Stores.Store> Stores { get; set; }
    public DbSet<Talabi.Stores.StoreUser> StoreUsers { get; set; }
    public DbSet<Talabi.Stores.StorePaymentAccount> StorePaymentAccounts { get; set; }

    // 3. Categories
    public DbSet<Talabi.Categories.Category> Categories { get; set; }
    public DbSet<Talabi.Categories.StoreCategory> StoreCategories { get; set; }

    // 4. Products & Inventory
    public DbSet<Talabi.Products.Product> Products { get; set; }
    public DbSet<Talabi.Products.ProductImage> ProductImages { get; set; }
    // public DbSet<Talabi.Products.Inventory> Inventories { get; set; }

    // 5. Carts
    public DbSet<Talabi.Carts.Cart> Carts { get; set; }
    public DbSet<Talabi.Carts.CartItem> CartItems { get; set; }

    // 6. Orders
    public DbSet<Talabi.Orders.OrderStatus> OrderStatuses { get; set; }
    public DbSet<Talabi.Orders.Order> Orders { get; set; }
    public DbSet<Talabi.Orders.OrderItem> OrderItems { get; set; }
    public DbSet<Talabi.Orders.OrderStatusHistory> OrderStatusHistories { get; set; }
    public DbSet<Talabi.Orders.CancellationReason> CancellationReasons { get; set; }
    public DbSet<Talabi.Orders.OrderRejection> OrderRejections { get; set; }
    public DbSet<Talabi.Orders.OrderCancellation> OrderCancellations { get; set; }

    // 7. Payments
    public DbSet<Talabi.Payments.PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Talabi.Payments.Payment> Payments { get; set; }
    public DbSet<Talabi.Payments.PaymentReceipt> PaymentReceipts { get; set; }

    // 8. Deliveries
    public DbSet<Talabi.Deliveries.Courier> Couriers { get; set; }
    public DbSet<Talabi.Deliveries.DeliveryAssignment> DeliveryAssignments { get; set; }
    public DbSet<Talabi.Deliveries.DeliveryConfirmation> DeliveryConfirmations { get; set; }

    // 9. Notifications & Media
    public DbSet<Talabi.Notifications.NotificationType> NotificationTypes { get; set; }
    public DbSet<Talabi.Notifications.AppNotification> Notifications { get; set; }
    public DbSet<Talabi.MediaFiles.MediaFile> MediaFiles { get; set; }

    // 10. Reviews & Favorites
    public DbSet<Talabi.Interactions.Review> Reviews { get; set; }
    public DbSet<Talabi.Interactions.Favorite> Favorites { get; set; }

    #endregion

    public TalabiDbContext(DbContextOptions<TalabiDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* تطبيق جميع ملفات IEntityTypeConfiguration المعرفة في طبقة EntityFrameworkCore تلقائياً */
        builder.ApplyConfigurationsFromAssembly(typeof(TalabiEntityFrameworkCoreModule).Assembly);
    }
}
