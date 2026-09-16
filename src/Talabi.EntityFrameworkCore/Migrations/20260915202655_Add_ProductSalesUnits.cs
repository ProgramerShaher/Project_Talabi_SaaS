using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talabi.Migrations
{
    /// <inheritdoc />
    public partial class Add_ProductSalesUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "AppCartItems");

            migrationBuilder.AddColumn<Guid>(
                name: "SalesUnitId",
                table: "AppOrderItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalesUnitId",
                table: "AppCartItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitName",
                table: "AppCartItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "حبة");

            migrationBuilder.CreateTable(
                name: "AppSalesUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_AppSalesUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProductSalesUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalesUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_AppProductSalesUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProductSalesUnits_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppProductSalesUnits_AppSalesUnits_SalesUnitId",
                        column: x => x.SalesUnitId,
                        principalTable: "AppSalesUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppOrderItems_SalesUnitId",
                table: "AppOrderItems",
                column: "SalesUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCartItems_SalesUnitId",
                table: "AppCartItems",
                column: "SalesUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId_SalesUnitId",
                table: "AppCartItems",
                columns: new[] { "CartId", "ProductId", "SalesUnitId" },
                unique: true,
                filter: "[SalesUnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppProductSalesUnits_SalesUnitId",
                table: "AppProductSalesUnits",
                column: "SalesUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSalesUnits_ProductId_IsDefault",
                table: "AppProductSalesUnits",
                columns: new[] { "ProductId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductSalesUnits_ProductId_SalesUnitId",
                table: "AppProductSalesUnits",
                columns: new[] { "ProductId", "SalesUnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesUnits_TenantId_Name",
                table: "AppSalesUnits",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_AppCartItems_AppSalesUnits_SalesUnitId",
                table: "AppCartItems",
                column: "SalesUnitId",
                principalTable: "AppSalesUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AppOrderItems_AppSalesUnits_SalesUnitId",
                table: "AppOrderItems",
                column: "SalesUnitId",
                principalTable: "AppSalesUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppCartItems_AppSalesUnits_SalesUnitId",
                table: "AppCartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppOrderItems_AppSalesUnits_SalesUnitId",
                table: "AppOrderItems");

            migrationBuilder.DropTable(
                name: "AppProductSalesUnits");

            migrationBuilder.DropTable(
                name: "AppSalesUnits");

            migrationBuilder.DropIndex(
                name: "IX_AppOrderItems_SalesUnitId",
                table: "AppOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_AppCartItems_SalesUnitId",
                table: "AppCartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductId_SalesUnitId",
                table: "AppCartItems");

            migrationBuilder.DropColumn(
                name: "SalesUnitId",
                table: "AppOrderItems");

            migrationBuilder.DropColumn(
                name: "SalesUnitId",
                table: "AppCartItems");

            migrationBuilder.DropColumn(
                name: "UnitName",
                table: "AppCartItems");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "AppCartItems",
                columns: new[] { "CartId", "ProductId" },
                unique: true);
        }
    }
}
