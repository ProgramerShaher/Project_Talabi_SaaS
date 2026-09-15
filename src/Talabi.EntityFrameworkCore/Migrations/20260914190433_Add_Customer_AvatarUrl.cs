using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talabi.Migrations
{
    /// <inheritdoc />
    public partial class Add_Customer_AvatarUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "AppCustomers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "AppCustomers");
        }
    }
}
