using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LNUBookShare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToChatMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedByReceiver",
                table: "chat_message",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedBySender",
                table: "chat_message",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeletedByReceiver",
                table: "chat_message");

            migrationBuilder.DropColumn(
                name: "IsDeletedBySender",
                table: "chat_message");
        }
    }
}
