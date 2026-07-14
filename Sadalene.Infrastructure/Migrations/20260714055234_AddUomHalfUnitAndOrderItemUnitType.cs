using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sadalene.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUomHalfUnitAndOrderItemUnitType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowsHalfUnit",
                table: "UomMasters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UnitType",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 1); // OrderItemUnitType.Full — existing rows are unambiguously "full" quantity
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowsHalfUnit",
                table: "UomMasters");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "OrderItems");
        }
    }
}
