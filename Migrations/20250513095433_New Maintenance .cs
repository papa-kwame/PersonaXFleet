using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class NewMaintenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaintenanceApprovals_MaintenanceRequestId",
                table: "MaintenanceApprovals");

            migrationBuilder.DropColumn(
                name: "ApprovedByUser",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "MaintenanceRequests");

            migrationBuilder.AddColumn<string>(
                name: "CurrentRouteId",
                table: "MaintenanceRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentStage",
                table: "MaintenanceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "MaintenanceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MaintenanceTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaintenanceRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceTransactions_MaintenanceRequests_MaintenanceRequestId",
                        column: x => x.MaintenanceRequestId,
                        principalTable: "MaintenanceRequests",
                        principalColumn: "MaintenanceId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_CurrentRouteId",
                table: "MaintenanceRequests",
                column: "CurrentRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceApprovals_MaintenanceRequestId",
                table: "MaintenanceApprovals",
                column: "MaintenanceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTransactions_MaintenanceRequestId",
                table: "MaintenanceTransactions",
                column: "MaintenanceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTransactions_UserId",
                table: "MaintenanceTransactions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Routes_CurrentRouteId",
                table: "MaintenanceRequests",
                column: "CurrentRouteId",
                principalTable: "Routes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Routes_CurrentRouteId",
                table: "MaintenanceRequests");

            migrationBuilder.DropTable(
                name: "MaintenanceTransactions");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequests_CurrentRouteId",
                table: "MaintenanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceApprovals_MaintenanceRequestId",
                table: "MaintenanceApprovals");

            migrationBuilder.DropColumn(
                name: "CurrentRouteId",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "CurrentStage",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "MaintenanceRequests");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUser",
                table: "MaintenanceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "MaintenanceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceApprovals_MaintenanceRequestId",
                table: "MaintenanceApprovals",
                column: "MaintenanceRequestId",
                unique: true);
        }
    }
}
