using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sadalene.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPackingTypeMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackingType",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "PackingTypeId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PackingTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackingTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_PackingTypeId",
                table: "Products",
                column: "PackingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingTypes_Name",
                table: "PackingTypes",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PackingTypes_PackingTypeId",
                table: "Products",
                column: "PackingTypeId",
                principalTable: "PackingTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_PackingTypes_PackingTypeId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "PackingTypes");

            migrationBuilder.DropIndex(
                name: "IX_Products_PackingTypeId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PackingTypeId",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "PackingType",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
