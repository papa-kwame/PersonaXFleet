using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class AnotherTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_AspNetUsers_RequestingUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Vehicles_VehicleId",
                table: "MaintenanceRequests");

            migrationBuilder.DropTable(
                name: "MaintenanceApprovals");

            migrationBuilder.DropTable(
                name: "MaintenanceComments");

            migrationBuilder.DropColumn(
                name: "IssueType",
                table: "MaintenanceRequests");

            migrationBuilder.RenameColumn(
                name: "TargetCompletionDate",
                table: "MaintenanceRequests",
                newName: "CompletionDate");

            migrationBuilder.RenameColumn(
                name: "RequestingUserId",
                table: "MaintenanceRequests",
                newName: "RequestedByUserId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "MaintenanceRequests",
                newName: "MaintenanceId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRequests_RequestingUserId",
                table: "MaintenanceRequests",
                newName: "IX_MaintenanceRequests_RequestedByUserId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "MaintenanceRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "MaintenanceRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AdminComments",
                table: "MaintenanceRequests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RequestType",
                table: "MaintenanceRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_AspNetUsers_RequestedByUserId",
                table: "MaintenanceRequests",
                column: "RequestedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Vehicles_VehicleId",
                table: "MaintenanceRequests",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_AspNetUsers_RequestedByUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Vehicles_VehicleId",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "AdminComments",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "RequestType",
                table: "MaintenanceRequests");

            migrationBuilder.RenameColumn(
                name: "RequestedByUserId",
                table: "MaintenanceRequests",
                newName: "RequestingUserId");

            migrationBuilder.RenameColumn(
                name: "CompletionDate",
                table: "MaintenanceRequests",
                newName: "TargetCompletionDate");

            migrationBuilder.RenameColumn(
                name: "MaintenanceId",
                table: "MaintenanceRequests",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRequests_RequestedByUserId",
                table: "MaintenanceRequests",
                newName: "IX_MaintenanceRequests_RequestingUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "MaintenanceRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "MaintenanceRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "IssueType",
                table: "MaintenanceRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MaintenanceApprovals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ApproverUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaintenanceRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceApprovals_AspNetUsers_ApproverUserId",
                        column: x => x.ApproverUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceApprovals_MaintenanceRequests_MaintenanceRequestId",
                        column: x => x.MaintenanceRequestId,
                        principalTable: "MaintenanceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceComments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaintenanceRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CommentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceComments_MaintenanceRequests_MaintenanceRequestId",
                        column: x => x.MaintenanceRequestId,
                        principalTable: "MaintenanceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceApprovals_ApproverUserId",
                table: "MaintenanceApprovals",
                column: "ApproverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceApprovals_MaintenanceRequestId",
                table: "MaintenanceApprovals",
                column: "MaintenanceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceComments_MaintenanceRequestId",
                table: "MaintenanceComments",
                column: "MaintenanceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceComments_UserId",
                table: "MaintenanceComments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_AspNetUsers_RequestingUserId",
                table: "MaintenanceRequests",
                column: "RequestingUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Vehicles_VehicleId",
                table: "MaintenanceRequests",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
