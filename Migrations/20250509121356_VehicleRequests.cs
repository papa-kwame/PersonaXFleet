using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class VehicleRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleRequests_AspNetUsers_UserId",
                table: "VehicleRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleRequests_Vehicles_VehicleId",
                table: "VehicleRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleRequests",
                table: "VehicleRequests");

            migrationBuilder.DropIndex(
                name: "IX_VehicleRequests_UserId",
                table: "VehicleRequests");

            migrationBuilder.DropIndex(
                name: "IX_VehicleRequests_VehicleId",
                table: "VehicleRequests");

            migrationBuilder.DropColumn(
                name: "AssignedVehicleId",
                table: "VehicleRequests");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "VehicleRequests");

            migrationBuilder.RenameTable(
                name: "VehicleRequests",
                newName: "VehicleRequest");

            migrationBuilder.RenameColumn(
                name: "RequestDate",
                table: "VehicleRequest",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "Purpose",
                table: "VehicleRequest",
                newName: "Reason");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "VehicleRequest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "VehicleRequest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "VehicleRequest",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "VehicleRequest",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "VehicleRequest",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleRequest",
                table: "VehicleRequest",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleRequest",
                table: "VehicleRequest");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "VehicleRequest");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "VehicleRequest");

            migrationBuilder.RenameTable(
                name: "VehicleRequest",
                newName: "VehicleRequests");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "VehicleRequests",
                newName: "RequestDate");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "VehicleRequests",
                newName: "Purpose");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "VehicleRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "VehicleRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "VehicleRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "AssignedVehicleId",
                table: "VehicleRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleId",
                table: "VehicleRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleRequests",
                table: "VehicleRequests",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleRequests_UserId",
                table: "VehicleRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleRequests_VehicleId",
                table: "VehicleRequests",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleRequests_AspNetUsers_UserId",
                table: "VehicleRequests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleRequests_Vehicles_VehicleId",
                table: "VehicleRequests",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");
        }
    }
}
