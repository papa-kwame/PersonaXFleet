using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentity6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VehicleId",
                table: "VehicleAssignmentRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAssignmentRequests_VehicleId",
                table: "VehicleAssignmentRequests",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleAssignmentRequests_Vehicles_VehicleId",
                table: "VehicleAssignmentRequests",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleAssignmentRequests_Vehicles_VehicleId",
                table: "VehicleAssignmentRequests");

            migrationBuilder.DropIndex(
                name: "IX_VehicleAssignmentRequests_VehicleId",
                table: "VehicleAssignmentRequests");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "VehicleAssignmentRequests");
        }
    }
}
