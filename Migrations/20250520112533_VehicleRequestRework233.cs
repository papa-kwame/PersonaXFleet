using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class VehicleRequestRework233 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleAssignmentTransactions_VehicleAssignmentRequests_RequestId",
                table: "VehicleAssignmentTransactions");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "VehicleAssignmentTransactions",
                newName: "VehicleAssignmentRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleAssignmentTransactions_RequestId",
                table: "VehicleAssignmentTransactions",
                newName: "IX_VehicleAssignmentTransactions_VehicleAssignmentRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleAssignmentTransactions_VehicleAssignmentRequests_VehicleAssignmentRequestId",
                table: "VehicleAssignmentTransactions",
                column: "VehicleAssignmentRequestId",
                principalTable: "VehicleAssignmentRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleAssignmentTransactions_VehicleAssignmentRequests_VehicleAssignmentRequestId",
                table: "VehicleAssignmentTransactions");

            migrationBuilder.RenameColumn(
                name: "VehicleAssignmentRequestId",
                table: "VehicleAssignmentTransactions",
                newName: "RequestId");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleAssignmentTransactions_VehicleAssignmentRequestId",
                table: "VehicleAssignmentTransactions",
                newName: "IX_VehicleAssignmentTransactions_RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleAssignmentTransactions_VehicleAssignmentRequests_RequestId",
                table: "VehicleAssignmentTransactions",
                column: "RequestId",
                principalTable: "VehicleAssignmentRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
