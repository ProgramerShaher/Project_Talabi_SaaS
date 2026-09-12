using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talabi.Migrations
{
    /// <inheritdoc />
    public partial class AddTalabiCoreEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppCancellationReasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TargetAudience = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCancellationReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsGlobal = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Level = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Path = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCategories_AppCategories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AppCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppCouriers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VehicleNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CurrentLatitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    CurrentLongitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    LastLocationUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false, defaultValue: 0m),
                    TotalDeliveries = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalEarnings = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCouriers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppCustomers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    LoyaltyPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "ar"),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCustomers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppMediaFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    StorageProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BucketName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ObjectKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PublicUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MediumUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OptimizedUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Hash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AccessLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMediaFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppNotificationTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Template = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNotificationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppOrderStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppOrderStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RequiresReceipt = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppStoreTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStoreTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppCustomerAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Building = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Floor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Apartment = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AdditionalDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCustomerAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCustomerAddresses_AppCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "AppCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppFavorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppFavorites_AppCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "AppCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RelatedEntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentVia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppNotifications_AppNotificationTypes_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalTable: "AppNotificationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppStores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    WorkingHoursJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinimumOrderAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DeliveryFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    AverageDeliveryTime = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 4),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false, defaultValue: 0m),
                    TotalReviews = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalOrders = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStores_AppStoreTypes_StoreTypeId",
                        column: x => x.StoreTypeId,
                        principalTable: "AppStoreTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppCarts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCarts_AppCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "AppCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppCarts_AppStores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "AppStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryAddressSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DeliveryFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    OrderStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    PaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StoreNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EstimatedDeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualDeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppOrders_AppCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "AppCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppOrders_AppOrderStatuses_OrderStatusId",
                        column: x => x.OrderStatusId,
                        principalTable: "AppOrderStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppOrders_AppStores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "AppStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppStoreCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GlobalCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomIconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStoreCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStoreCategories_AppCategories_GlobalCategoryId",
                        column: x => x.GlobalCategoryId,
                        principalTable: "AppCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppStoreCategories_AppStoreCategories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AppStoreCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppStoreCategories_AppStores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "AppStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppStoreUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStoreUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStoreUsers_AppStores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "AppStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppDeliveryAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    AssignedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PickupAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedArrival = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DistanceKm = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    DeliveryFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CourierEarning = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDeliveryAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppDeliveryAssignments_AppCouriers_CourierId",
                        column: x => x.CourierId,
                        principalTable: "AppCouriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppDeliveryAssignments_AppOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "AppOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppOrderCancellations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancellationReasonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancelledByRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PreviousStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppOrderCancellations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppOrderCancellations_AppCancellationReasons_CancellationReasonId",
                        column: x => x.CancellationReasonId,
                        principalTable: "AppCancellationReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppOrderCancellations_AppOrderStatuses_PreviousStatusId",
                        column: x => x.PreviousStatusId,
                        principalTable: "AppOrderStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppOrderCancellations_AppOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "AppOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppOrderRejections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RejectedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RejectionReasonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppOrderRejections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppOrderRejections_AppCancellationReasons_RejectionReasonId",
                        column: x => x.RejectionReasonId,
                        principalTable: "AppCancellationReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppOrderRejections_AppOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "AppOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppOrderStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedByRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppOrderStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppOrderStatusHistories_AppOrderStatuses_FromStatusId",
                        column: x => x.FromStatusId,
                        principalTable: "AppOrderStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppOrderStatusHistories_AppOrderStatuses_ToStatusId",
                        column: x => x.ToStatusId,
                        principalTable: "AppOrderStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppOrderStatusHistories_AppOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "AppOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "SAR"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    TransactionReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GatewayResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppPayments_AppOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "AppOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppPayments_AppPaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "AppPaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StoreRating = table.Column<int>(type: "int", nullable: false),
                    CourierRating = table.Column<int>(type: "int", nullable: true),
                    ProductQualityRating = table.Column<int>(type: "int", nullable: true),
                    DeliverySpeedRating = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StoreReply = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StoreRepliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppReviews", x => x.Id);
                    table.CheckConstraint("CK_Reviews_StoreRating", "[StoreRating] BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_AppReviews_AppCouriers_CourierId",
                        column: x => x.CourierId,
                        principalTable: "AppCouriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppReviews_AppCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "AppCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppReviews_AppOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "AppOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppReviews_AppStores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "AppStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    WeightUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Origin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MainImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MinOrderQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MaxOrderQuantity = table.Column<int>(type: "int", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProducts_AppStoreCategories_StoreCategoryId",
                        column: x => x.StoreCategoryId,
                        principalTable: "AppStoreCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProducts_AppStores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "AppStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppDeliveryConfirmations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfirmedBy = table.Column<int>(type: "int", nullable: false),
                    ConfirmedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VerificationCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SignatureUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhotoProofUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDeliveryConfirmations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppDeliveryConfirmations_AppDeliveryAssignments_DeliveryAssignmentId",
                        column: x => x.DeliveryAssignmentId,
                        principalTable: "AppDeliveryAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppPaymentReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransactionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    VerificationStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    VerifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPaymentReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppPaymentReceipts_AppPayments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "AppPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppCartItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPriceAtAddition = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCartItems", x => x.Id);
                    table.CheckConstraint("CK_CartItems_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_AppCartItems_AppCarts_CartId",
                        column: x => x.CartId,
                        principalTable: "AppCarts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppCartItems_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppInventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReservedQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MinStockLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    MaxStockLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1000),
                    ReorderLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    WarehouseLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastRestockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsTracked = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppInventory", x => x.Id);
                    table.CheckConstraint("CK_Inventory_CurrentQuantity", "[CurrentQuantity] >= 0");
                    table.CheckConstraint("CK_Inventory_ReservedQuantity", "[ReservedQuantity] >= 0");
                    table.ForeignKey(
                        name: "FK_AppInventory_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppInventory_AppStores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "AppStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppOrderItems", x => x.Id);
                    table.CheckConstraint("CK_OrderItems_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_AppOrderItems_AppOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "AppOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppOrderItems_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProductImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AltText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProductImages_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCartItems_CartId",
                table: "AppCartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCartItems_ProductId",
                table: "AppCartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCarts_StoreId",
                table: "AppCarts",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CustomerId_StoreId_IsActive",
                table: "AppCarts",
                columns: new[] { "CustomerId", "StoreId", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsGlobal_IsActive",
                table: "AppCategories",
                columns: new[] { "IsGlobal", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                table: "AppCategories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "AppCategories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCouriers_UserId",
                table: "AppCouriers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddresses_CustomerId_IsDefault",
                table: "AppCustomerAddresses",
                columns: new[] { "CustomerId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_UserId",
                table: "AppCustomers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDeliveryAssignments_OrderId",
                table: "AppDeliveryAssignments",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAssignments_CourierId_Status",
                table: "AppDeliveryAssignments",
                columns: new[] { "CourierId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppDeliveryConfirmations_DeliveryAssignmentId",
                table: "AppDeliveryConfirmations",
                column: "DeliveryAssignmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_CustomerId_EntityType_EntityId",
                table: "AppFavorites",
                columns: new[] { "CustomerId", "EntityType", "EntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppInventory_ProductId",
                table: "AppInventory",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_StoreId_ProductId",
                table: "AppInventory",
                columns: new[] { "StoreId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_EntityType_EntityId",
                table: "AppMediaFiles",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Hash",
                table: "AppMediaFiles",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Status",
                table: "AppMediaFiles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppNotifications_NotificationTypeId",
                table: "AppNotifications",
                column: "NotificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId_IsRead_CreatedAt",
                table: "AppNotifications",
                columns: new[] { "RecipientUserId", "IsRead", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNotificationTypes_Name",
                table: "AppNotificationTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppOrderCancellations_CancellationReasonId",
                table: "AppOrderCancellations",
                column: "CancellationReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_AppOrderCancellations_OrderId",
                table: "AppOrderCancellations",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppOrderCancellations_PreviousStatusId",
                table: "AppOrderCancellations",
                column: "PreviousStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AppOrderItems_OrderId",
                table: "AppOrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppOrderItems_ProductId",
                table: "AppOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppOrderRejections_OrderId",
                table: "AppOrderRejections",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppOrderRejections_RejectionReasonId",
                table: "AppOrderRejections",
                column: "RejectionReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_AppOrders_OrderStatusId",
                table: "AppOrders",
                column: "OrderStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId_CreatedAt",
                table: "AppOrders",
                columns: new[] { "CustomerId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "AppOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StoreId_OrderStatusId",
                table: "AppOrders",
                columns: new[] { "StoreId", "OrderStatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOrderStatuses_Name",
                table: "AppOrderStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppOrderStatusHistories_FromStatusId",
                table: "AppOrderStatusHistories",
                column: "FromStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AppOrderStatusHistories_ToStatusId",
                table: "AppOrderStatusHistories",
                column: "ToStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_OrderId_CreatedAt",
                table: "AppOrderStatusHistories",
                columns: new[] { "OrderId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPaymentMethods_Name",
                table: "AppPaymentMethods",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPaymentReceipts_PaymentId",
                table: "AppPaymentReceipts",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPayments_OrderId",
                table: "AppPayments",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPayments_PaymentMethodId",
                table: "AppPayments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_DisplayOrder",
                table: "AppProductImages",
                columns: new[] { "ProductId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProducts_StoreCategoryId",
                table: "AppProducts",
                column: "StoreCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreId_Barcode",
                table: "AppProducts",
                columns: new[] { "StoreId", "Barcode" },
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreId_IsActive",
                table: "AppProducts",
                columns: new[] { "StoreId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreId_SKU",
                table: "AppProducts",
                columns: new[] { "StoreId", "SKU" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppReviews_CourierId",
                table: "AppReviews",
                column: "CourierId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviews_OrderId",
                table: "AppReviews",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CustomerId",
                table: "AppReviews",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_StoreId_IsVisible",
                table: "AppReviews",
                columns: new[] { "StoreId", "IsVisible" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreCategories_GlobalCategoryId",
                table: "AppStoreCategories",
                column: "GlobalCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreCategories_ParentId",
                table: "AppStoreCategories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreCategories_StoreId_ParentId",
                table: "AppStoreCategories",
                columns: new[] { "StoreId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Stores_OwnerId",
                table: "AppStores",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Slug",
                table: "AppStores",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_StoreTypeId_IsActive",
                table: "AppStores",
                columns: new[] { "StoreTypeId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreTypes_Name",
                table: "AppStoreTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreUsers_StoreId_UserId",
                table: "AppStoreUsers",
                columns: new[] { "StoreId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppCartItems");

            migrationBuilder.DropTable(
                name: "AppCustomerAddresses");

            migrationBuilder.DropTable(
                name: "AppDeliveryConfirmations");

            migrationBuilder.DropTable(
                name: "AppFavorites");

            migrationBuilder.DropTable(
                name: "AppInventory");

            migrationBuilder.DropTable(
                name: "AppMediaFiles");

            migrationBuilder.DropTable(
                name: "AppNotifications");

            migrationBuilder.DropTable(
                name: "AppOrderCancellations");

            migrationBuilder.DropTable(
                name: "AppOrderItems");

            migrationBuilder.DropTable(
                name: "AppOrderRejections");

            migrationBuilder.DropTable(
                name: "AppOrderStatusHistories");

            migrationBuilder.DropTable(
                name: "AppPaymentReceipts");

            migrationBuilder.DropTable(
                name: "AppProductImages");

            migrationBuilder.DropTable(
                name: "AppReviews");

            migrationBuilder.DropTable(
                name: "AppStoreUsers");

            migrationBuilder.DropTable(
                name: "AppCarts");

            migrationBuilder.DropTable(
                name: "AppDeliveryAssignments");

            migrationBuilder.DropTable(
                name: "AppNotificationTypes");

            migrationBuilder.DropTable(
                name: "AppCancellationReasons");

            migrationBuilder.DropTable(
                name: "AppPayments");

            migrationBuilder.DropTable(
                name: "AppProducts");

            migrationBuilder.DropTable(
                name: "AppCouriers");

            migrationBuilder.DropTable(
                name: "AppOrders");

            migrationBuilder.DropTable(
                name: "AppPaymentMethods");

            migrationBuilder.DropTable(
                name: "AppStoreCategories");

            migrationBuilder.DropTable(
                name: "AppCustomers");

            migrationBuilder.DropTable(
                name: "AppOrderStatuses");

            migrationBuilder.DropTable(
                name: "AppCategories");

            migrationBuilder.DropTable(
                name: "AppStores");

            migrationBuilder.DropTable(
                name: "AppStoreTypes");
        }
    }
}
