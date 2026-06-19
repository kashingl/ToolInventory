using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolInventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SecurityAndDataIntegrityImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Checkouts_User_UserId",
                table: "Checkouts");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Checkouts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_Barcode",
                table: "Tools",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MaintenanceRecord_Cost",
                table: "MaintenanceRecords",
                sql: "[Cost] IS NULL OR [Cost] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Checkouts_ReturnDate",
                table: "Checkouts",
                sql: "[ActualReturnDate] IS NULL OR [ActualReturnDate] >= [CheckoutDate]");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tools_Barcode",
                table: "Tools");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MaintenanceRecord_Cost",
                table: "MaintenanceRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Checkouts_ReturnDate",
                table: "Checkouts");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Checkouts",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Checkouts_User_UserId",
                table: "Checkouts",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
