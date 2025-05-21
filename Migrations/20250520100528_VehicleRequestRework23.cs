using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class VehicleRequestRework23 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleAssignmentRequests_Vehicles_VehicleId",
                table: "VehicleAssignmentRequests");

            migrationBuilder.DropColumn(
                name: "AdditionalNotes",
                table: "VehicleAssignmentRequests");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "VehicleAssignmentRequests");

            migrationBuilder.DropColumn(
                name: "AssignmentDate",
                table: "VehicleAssignmentRequests");

            migrationBuilder.DropColumn(
                name: "AssignmentReason",
                table: "VehicleAssignmentRequests");

            migrationBuilder.DropColumn(
                name: "ExpectedDuration",
                table: "VehicleAssignmentRequests");

            migrationBuilder.DropColumn(
                name: "ProcessedDate",
                table: "VehicleAssignmentRequests");

            migrationBuilder.DropColumn(
                name: "UnassignmentDate",
                table: "VehicleAssignmentRequests");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "VehicleAssignmentRequests",
                newName: "CurrentRouteId");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "VehicleAssignmentRequests",
                newName: "RequestReason");

            migrationBuilder.RenameColumn(
                name: "ProcessedBy",
                table: "VehicleAssignmentRequests",
                newName: "CurrentStage");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleAssignmentRequests_VehicleId",
                table: "VehicleAssignmentRequests",
                newName: "IX_VehicleAssignmentRequests_CurrentRouteId");

            migrationBuilder.CreateTable(
                name: "VehicleAssignmentTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleAssignmentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleAssignmentTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleAssignmentTransactions_VehicleAssignmentRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "VehicleAssignmentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAssignmentTransactions_RequestId",
                table: "VehicleAssignmentTransactions",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAssignmentTransactions_UserId",
                table: "VehicleAssignmentTransactions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleAssignmentRequests_Routes_CurrentRouteId",
                table: "VehicleAssignmentRequests",
                column: "CurrentRouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleAssignmentRequests_Routes_CurrentRouteId",
                table: "VehicleAssignmentRequests");

            migrationBuilder.DropTable(
                name: "VehicleAssignmentTransactions");

            migrationBuilder.RenameColumn(
                name: "RequestReason",
                table: "VehicleAssignmentRequests",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "CurrentStage",
                table: "VehicleAssignmentRequests",
                newName: "ProcessedBy");

            migrationBuilder.RenameColumn(
                name: "CurrentRouteId",
                table: "VehicleAssignmentRequests",
                newName: "VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleAssignmentRequests_CurrentRouteId",
                table: "VehicleAssignmentRequests",
                newName: "IX_VehicleAssignmentRequests_VehicleId");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalNotes",
                table: "VehicleAssignmentRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "VehicleAssignmentRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignmentDate",
                table: "VehicleAssignmentRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignmentReason",
                table: "VehicleAssignmentRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExpectedDuration",
                table: "VehicleAssignmentRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedDate",
                table: "VehicleAssignmentRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnassignmentDate",
                table: "VehicleAssignmentRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleAssignmentRequests_Vehicles_VehicleId",
                table: "VehicleAssignmentRequests",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
