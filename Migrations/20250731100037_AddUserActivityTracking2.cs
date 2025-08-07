using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class AddUserActivityTracking2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastActivityDate",
                table: "AspNetUsers");
        }
    }
}
