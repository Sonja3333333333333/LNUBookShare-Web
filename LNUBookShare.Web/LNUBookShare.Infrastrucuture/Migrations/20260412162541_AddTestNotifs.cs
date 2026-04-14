using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LNUBookShare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestNotifs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "notification",
                columns: new[] { "notification_id", "book_id", "created_at", "message_", "user_id" },
                values: new object[] { 1, 1, new DateTime(2024, 4, 12, 10, 0, 0, 0, DateTimeKind.Utc), "Книга 'Чиста Архітектура' тепер доступна для бронювання!", 9 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "notification",
                keyColumn: "notification_id",
                keyValue: 1);
        }
    }
}
