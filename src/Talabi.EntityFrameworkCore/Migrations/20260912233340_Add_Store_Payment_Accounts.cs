using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talabi.Migrations
{
    /// <inheritdoc />
    public partial class Add_Store_Payment_Accounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppProducts_AppStoreCategories_StoreCategoryId",
                table: "AppProducts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_CustomerId_StoreId_IsActive",
                table: "AppCarts");

            migrationBuilder.DropIndex(
                name: "IX_AppCartItems_CartId",
                table: "AppCartItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreCategoryId",
                table: "AppProducts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "PaymentReceiptUrl",
                table: "AppOrders",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AppCustomers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AppCustomerAddresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalTotal",
                table: "AppCarts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "AppCarts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDiscount",
                table: "AppCarts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "AppCartItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AppStorePaymentAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
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
                    table.PrimaryKey("PK_AppStorePaymentAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStorePaymentAccounts_AppStores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "AppStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CustomerId_IsActive",
                table: "AppCarts",
                columns: new[] { "CustomerId", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "AppCartItems",
                columns: new[] { "CartId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppStorePaymentAccounts_StoreId",
                table: "AppStorePaymentAccounts",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppProducts_AppStoreCategories_StoreCategoryId",
                table: "AppProducts",
                column: "StoreCategoryId",
                principalTable: "AppStoreCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppProducts_AppStoreCategories_StoreCategoryId",
                table: "AppProducts");

            migrationBuilder.DropTable(
                name: "AppStorePaymentAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_CustomerId_IsActive",
                table: "AppCarts");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "AppCartItems");

            migrationBuilder.DropColumn(
                name: "PaymentReceiptUrl",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AppCustomers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AppCustomerAddresses");

            migrationBuilder.DropColumn(
                name: "FinalTotal",
                table: "AppCarts");

            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "AppCarts");

            migrationBuilder.DropColumn(
                name: "TotalDiscount",
                table: "AppCarts");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "AppCartItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreCategoryId",
                table: "AppProducts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CustomerId_StoreId_IsActive",
                table: "AppCarts",
                columns: new[] { "CustomerId", "StoreId", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AppCartItems_CartId",
                table: "AppCartItems",
                column: "CartId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppProducts_AppStoreCategories_StoreCategoryId",
                table: "AppProducts",
                column: "StoreCategoryId",
                principalTable: "AppStoreCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
