using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class WorkOrderUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_AspNetUsers_AssignedTechnicianId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_AssignedTechnicianId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "AssignedTechnicianId",
                table: "WorkOrders");

            migrationBuilder.AddColumn<string>(
                name: "AssignedMechanicId",
                table: "WorkOrders",
                type: "nvarchar(450)",
                nullable: true); // Make sure it's nullable

            migrationBuilder.AddColumn<string>(
                name: "AssignedMechanicId1",
                table: "WorkOrders",
                type: "nvarchar(450)",
                nullable: true); // Make sure it's nullable

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignmentDate",
                table: "WorkOrders",
                type: "datetime2",
                nullable: true);

            // Update invalid AssignedMechanicId values to NULL
            migrationBuilder.Sql(@"
        UPDATE WorkOrders
        SET AssignedMechanicId = NULL
        WHERE AssignedMechanicId NOT IN (
            SELECT Id
            FROM AspNetUsers
            WHERE Id IS NOT NULL
        )");

            // Update invalid AssignedMechanicId1 values to NULL
            migrationBuilder.Sql(@"
        UPDATE WorkOrders
        SET AssignedMechanicId1 = NULL
        WHERE AssignedMechanicId1 NOT IN (
            SELECT Id
            FROM AspNetUsers
            WHERE Id IS NOT NULL
        )");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AssignedMechanicId",
                table: "WorkOrders",
                column: "AssignedMechanicId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_AspNetUsers_AssignedMechanicId",
                table: "WorkOrders",
                column: "AssignedMechanicId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_AspNetUsers_AssignedMechanicId1",
                table: "WorkOrders",
                column: "AssignedMechanicId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_AspNetUsers_AssignedMechanicId",
                table: "WorkOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_AspNetUsers_AssignedMechanicId1",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_AssignedMechanicId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_AssignedMechanicId1",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "AssignedMechanicId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "AssignedMechanicId1",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "AssignmentDate",
                table: "WorkOrders");

            migrationBuilder.AddColumn<string>(
                name: "AssignedTechnicianId",
                table: "WorkOrders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AssignedTechnicianId",
                table: "WorkOrders",
                column: "AssignedTechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_AspNetUsers_AssignedTechnicianId",
                table: "WorkOrders",
                column: "AssignedTechnicianId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
