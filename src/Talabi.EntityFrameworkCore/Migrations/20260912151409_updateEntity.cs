using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talabi.Migrations
{
    /// <inheritdoc />
    public partial class updateEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppReviews_AppCouriers_CourierId",
                table: "AppReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_AppReviews_AppOrders_OrderId",
                table: "AppReviews");

            migrationBuilder.DropTable(
                name: "AppInventory");

            migrationBuilder.DropIndex(
                name: "IX_AppReviews_CourierId",
                table: "AppReviews");

            migrationBuilder.DropIndex(
                name: "IX_AppReviews_OrderId",
                table: "AppReviews");

            migrationBuilder.DropIndex(
                name: "IX_Products_StoreId_Barcode",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "AppStores");

            migrationBuilder.DropColumn(
                name: "CourierId",
                table: "AppReviews");

            migrationBuilder.DropColumn(
                name: "CourierRating",
                table: "AppReviews");

            migrationBuilder.DropColumn(
                name: "DeliverySpeedRating",
                table: "AppReviews");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "AppReviews");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Origin",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "WeightUnit",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "CourierEarning",
                table: "AppDeliveryAssignments");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "AppDeliveryAssignments");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "AppCouriers");

            migrationBuilder.DropColumn(
                name: "TotalEarnings",
                table: "AppCouriers");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AppCustomerAddresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Governorate",
                table: "AppCustomerAddresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "AppCustomerAddresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "AppCustomerAddresses");

            migrationBuilder.DropColumn(
                name: "Governorate",
                table: "AppCustomerAddresses");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "AppCustomerAddresses");

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "AppStores",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "CourierId",
                table: "AppReviews",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CourierRating",
                table: "AppReviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliverySpeedRating",
                table: "AppReviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "AppReviews",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "AppProducts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "AppProducts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "AppProducts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Origin",
                table: "AppProducts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "AppProducts",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeightUnit",
                table: "AppProducts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "AppOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CourierEarning",
                table: "AppDeliveryAssignments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "AppDeliveryAssignments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "AppCouriers",
                type: "decimal(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalEarnings",
                table: "AppCouriers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AppInventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsTracked = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastRestockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxStockLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1000),
                    MinStockLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    ReorderLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    ReservedQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WarehouseLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
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
                name: "IX_Products_StoreId_Barcode",
                table: "AppProducts",
                columns: new[] { "StoreId", "Barcode" },
                unique: true,
                filter: "[Barcode] IS NOT NULL");

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

            migrationBuilder.AddForeignKey(
                name: "FK_AppReviews_AppCouriers_CourierId",
                table: "AppReviews",
                column: "CourierId",
                principalTable: "AppCouriers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppReviews_AppOrders_OrderId",
                table: "AppReviews",
                column: "OrderId",
                principalTable: "AppOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
