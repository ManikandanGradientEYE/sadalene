using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sadalene.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDivisionCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Divisions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Divisions",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: null);

            migrationBuilder.UpdateData(
                table: "Divisions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: null);

            migrationBuilder.UpdateData(
                table: "Divisions",
                keyColumn: "Id",
                keyValue: 3,
                column: "Code",
                value: null);

            migrationBuilder.UpdateData(
                table: "Divisions",
                keyColumn: "Id",
                keyValue: 4,
                column: "Code",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_Code",
                table: "Divisions",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Divisions_Code",
                table: "Divisions");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Divisions");
        }
    }
}
