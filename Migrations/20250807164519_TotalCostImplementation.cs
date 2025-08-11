using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class TotalCostImplementation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotatalCostofRepair",
                table: "MaintenanceRequests",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotatalCostofRepair",
                table: "MaintenanceRequests");
        }
    }
}
