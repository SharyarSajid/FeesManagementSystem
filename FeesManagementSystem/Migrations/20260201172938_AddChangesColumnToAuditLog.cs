using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeesManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddChangesColumnToAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Changes",
                table: "StudentAuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Changes",
                table: "StudentAuditLogs");
        }
    }
}
