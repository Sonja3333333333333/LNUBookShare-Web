using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LNUBookShare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "UserReports",
                type: "text",
                nullable: false,
                defaultValue: "Pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserReports");
        }
    }
}
