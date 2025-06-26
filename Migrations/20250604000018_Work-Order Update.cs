﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class WorkOrderUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_AspNetUsers_AssignedTechnicianId",
                table: "WorkOrders");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedTechnicianId",
                table: "WorkOrders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_AspNetUsers_AssignedTechnicianId",
                table: "WorkOrders",
                column: "AssignedTechnicianId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_AspNetUsers_AssignedTechnicianId",
                table: "WorkOrders");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedTechnicianId",
                table: "WorkOrders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_AspNetUsers_AssignedTechnicianId",
                table: "WorkOrders",
                column: "AssignedTechnicianId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
