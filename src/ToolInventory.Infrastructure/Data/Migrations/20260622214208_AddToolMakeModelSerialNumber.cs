using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolInventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddToolMakeModelSerialNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Make",
                table: "Tools",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Tools",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "Tools",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Make",
                table: "Tools");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Tools");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "Tools");
        }
    }
}
