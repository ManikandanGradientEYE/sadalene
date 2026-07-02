using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sadalene.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDivisionUnitOfMeasure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DivisionUnitOfMeasures");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DivisionUnitOfMeasures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    UnitName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DivisionUnitOfMeasures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DivisionUnitOfMeasures_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DivisionUnitOfMeasures",
                columns: new[] { "Id", "CreatedAt", "DivisionId", "IsActive", "IsDefault", "UnitName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, true, true, "Full Set", null },
                    { 2, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, true, false, "Half Set", null },
                    { 3, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, true, true, "Taka", null },
                    { 4, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, true, true, "No. of Boxes", null },
                    { 5, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, true, true, "Meters", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DivisionUnitOfMeasures_DivisionId",
                table: "DivisionUnitOfMeasures",
                column: "DivisionId");
        }
    }
}
