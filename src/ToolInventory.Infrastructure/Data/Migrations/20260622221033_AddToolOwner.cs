using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolInventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddToolOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Tools",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Tools");
        }
    }
}
