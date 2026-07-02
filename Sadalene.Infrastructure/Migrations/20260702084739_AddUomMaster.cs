using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sadalene.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUomMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                table: "SubCategories");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "UomMasterId",
                table: "SubCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UomMasterId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UomMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UomMasters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_UomMasterId",
                table: "SubCategories",
                column: "UomMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UomMasterId",
                table: "Products",
                column: "UomMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_UomMasters_Name",
                table: "UomMasters",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_UomMasters_UomMasterId",
                table: "Products",
                column: "UomMasterId",
                principalTable: "UomMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubCategories_UomMasters_UomMasterId",
                table: "SubCategories",
                column: "UomMasterId",
                principalTable: "UomMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_UomMasters_UomMasterId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_SubCategories_UomMasters_UomMasterId",
                table: "SubCategories");

            migrationBuilder.DropTable(
                name: "UomMasters");

            migrationBuilder.DropIndex(
                name: "IX_SubCategories_UomMasterId",
                table: "SubCategories");

            migrationBuilder.DropIndex(
                name: "IX_Products_UomMasterId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UomMasterId",
                table: "SubCategories");

            migrationBuilder.DropColumn(
                name: "UomMasterId",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                table: "SubCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
