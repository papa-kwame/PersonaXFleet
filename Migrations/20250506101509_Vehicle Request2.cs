using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class VehicleRequest2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleRequests_Vehicles_VehicleId",
                table: "VehicleRequests");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "VehicleRequests");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "VehicleRequests");

            migrationBuilder.RenameColumn(
                name: "RequestorName",
                table: "VehicleRequests",
                newName: "Purpose");

            migrationBuilder.AlterColumn<string>(
                name: "VehicleId",
                table: "VehicleRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "AssignedVehicleId",
                table: "VehicleRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleRequests_Vehicles_VehicleId",
                table: "VehicleRequests",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleRequests_Vehicles_VehicleId",
                table: "VehicleRequests");

            migrationBuilder.DropColumn(
                name: "AssignedVehicleId",
                table: "VehicleRequests");

            migrationBuilder.RenameColumn(
                name: "Purpose",
                table: "VehicleRequests",
                newName: "RequestorName");

            migrationBuilder.AlterColumn<string>(
                name: "VehicleId",
                table: "VehicleRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "VehicleRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "VehicleRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleRequests_Vehicles_VehicleId",
                table: "VehicleRequests",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
