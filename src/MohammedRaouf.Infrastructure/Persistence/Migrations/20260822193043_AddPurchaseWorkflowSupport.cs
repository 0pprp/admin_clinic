using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MohammedRaouf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseWorkflowSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseRequestNumberCounters",
                columns: table => new
                {
                    Year = table.Column<int>(type: "integer", nullable: false),
                    LastValue = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequestNumberCounters", x => x.Year);
                });

            migrationBuilder.CreateIndex(
                name: "UX_PurchaseRequests_OpenUserCourse",
                table: "PurchaseRequests",
                columns: new[] { "UserId", "CourseId" },
                unique: true,
                filter: "\"Status\" IN ('Pending', 'Contacted', 'AwaitingPayment', 'PaymentReceived', 'ActivationCodeIssued')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseRequestNumberCounters");

            migrationBuilder.DropIndex(
                name: "UX_PurchaseRequests_OpenUserCourse",
                table: "PurchaseRequests");
        }
    }
}
