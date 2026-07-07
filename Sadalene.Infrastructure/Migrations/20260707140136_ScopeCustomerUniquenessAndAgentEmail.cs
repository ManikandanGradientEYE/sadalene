using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sadalene.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScopeCustomerUniquenessAndAgentEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_Phone",
                table: "Customers");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true,
                filter: "[AgentId] IS NULL AND [Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Phone",
                table: "Customers",
                column: "Phone",
                unique: true,
                filter: "[AgentId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_Email",
                table: "Agents",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_Email",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Phone",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Agents_Email",
                table: "Agents");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Phone",
                table: "Customers",
                column: "Phone",
                unique: true);
        }
    }
}
