using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class ApprovalCommentsIds3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceComments_MaintenanceRequests_MaintenanceRequestId",
                table: "MaintenanceComments");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceComments_MaintenanceRequests_MaintenanceRequestId",
                table: "MaintenanceComments",
                column: "MaintenanceRequestId",
                principalTable: "MaintenanceRequests",
                principalColumn: "MaintenanceId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceComments_MaintenanceRequests_MaintenanceRequestId",
                table: "MaintenanceComments");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceComments_MaintenanceRequests_MaintenanceRequestId",
                table: "MaintenanceComments",
                column: "MaintenanceRequestId",
                principalTable: "MaintenanceRequests",
                principalColumn: "MaintenanceId");
        }
    }
}
