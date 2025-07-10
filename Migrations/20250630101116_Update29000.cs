﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class Update29000 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceSchedules_AspNetUsers_AssignedMechanicId",
                table: "MaintenanceSchedules");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedMechanicId",
                table: "MaintenanceSchedules",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceSchedules_AspNetUsers_AssignedMechanicId",
                table: "MaintenanceSchedules",
                column: "AssignedMechanicId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceSchedules_AspNetUsers_AssignedMechanicId",
                table: "MaintenanceSchedules");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedMechanicId",
                table: "MaintenanceSchedules",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceSchedules_AspNetUsers_AssignedMechanicId",
                table: "MaintenanceSchedules",
                column: "AssignedMechanicId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
