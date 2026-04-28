using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LNUBookShare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "report",
                columns: table => new
                {
                    report_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sender_id = table.Column<int>(type: "integer", nullable: false),
                    reported_user_id = table.Column<int>(type: "integer", nullable: false),
                    context_ = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                },
                constraints: table =>
                {
                    table.PrimaryKey("report_pkey", x => x.report_id);
                    table.ForeignKey(
                        name: "report_reported_user_id_fkey",
                        column: x => x.reported_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "report_sender_id_fkey",
                        column: x => x.sender_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "notification",
                keyColumn: "notification_id",
                keyValue: 1,
                column: "user_id",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_report_reported_user_id",
                table: "report",
                column: "reported_user_id");

            migrationBuilder.CreateIndex(
                name: "report_sender_reported_unique",
                table: "report",
                columns: new[] { "sender_id", "reported_user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "report");

            migrationBuilder.UpdateData(
                table: "notification",
                keyColumn: "notification_id",
                keyValue: 1,
                column: "user_id",
                value: 9);
        }
    }
}
