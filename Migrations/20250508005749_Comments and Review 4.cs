using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaXFleet.Migrations
{
    /// <inheritdoc />
    public partial class CommentsandReview4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ActualCost",
                table: "MaintenanceHistories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalComments",
                table: "MaintenanceHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUser",
                table: "MaintenanceHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "MaintenanceHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AssignedToUserId",
                table: "MaintenanceHistories",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceHistories_AssignedToUserId",
                table: "MaintenanceHistories",
                column: "AssignedToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceHistories_AspNetUsers_AssignedToUserId",
                table: "MaintenanceHistories",
                column: "AssignedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AlterColumn<string>(
             name: "ApprovalComments",
             table: "MaintenanceHistories",
             nullable: true,
             oldClrType: typeof(string),
             oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceHistories_AspNetUsers_AssignedToUserId",
                table: "MaintenanceHistories");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceHistories_AssignedToUserId",
                table: "MaintenanceHistories");

            migrationBuilder.DropColumn(
                name: "ActualCost",
                table: "MaintenanceHistories");

            migrationBuilder.DropColumn(
                name: "ApprovalComments",
                table: "MaintenanceHistories");

            migrationBuilder.DropColumn(
                name: "ApprovedByUser",
                table: "MaintenanceHistories");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "MaintenanceHistories");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "MaintenanceHistories");
           
            migrationBuilder.AlterColumn<string>(
           name: "ApprovalComments",
           table: "MaintenanceHistories",
           type: "nvarchar(max)",
           nullable: false,
           oldClrType: typeof(string),
           oldNullable: true);
        }

    }
}
