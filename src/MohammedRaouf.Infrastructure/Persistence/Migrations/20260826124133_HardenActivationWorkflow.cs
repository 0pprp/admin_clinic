using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MohammedRaouf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HardenActivationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActivationCodes_PurchaseRequestId",
                table: "ActivationCodes");

            migrationBuilder.CreateIndex(
                name: "UX_ActivationCodes_ActivePurchaseRequest",
                table: "ActivationCodes",
                column: "PurchaseRequestId",
                unique: true,
                filter: "\"Status\" = 'Active'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_ActivationCodes_ActivePurchaseRequest",
                table: "ActivationCodes");

            migrationBuilder.CreateIndex(
                name: "IX_ActivationCodes_PurchaseRequestId",
                table: "ActivationCodes",
                column: "PurchaseRequestId");
        }
    }
}
