using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LNUBookShare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_avatar_id_fkey",
                table: "user");

            migrationBuilder.DropIndex(
                name: "idx_user_review_owner",
                table: "user_review");

            migrationBuilder.DropIndex(
                name: "user_api_token_key",
                table: "user");

            migrationBuilder.DropIndex(
                name: "user_email_key",
                table: "user");

            migrationBuilder.DropIndex(
                name: "role_role_name_key",
                table: "role");

            migrationBuilder.DropIndex(
                name: "idx_chat_conversation",
                table: "chat_message");

            migrationBuilder.DropIndex(
                name: "book_isbn_key",
                table: "book");

            migrationBuilder.DropColumn(
                name: "api_token",
                table: "user");

            migrationBuilder.DropColumn(
                name: "avg_rating",
                table: "user");

            migrationBuilder.DropColumn(
                name: "is_email_confirmed",
                table: "user");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "user");

            migrationBuilder.RenameIndex(
                name: "idx_book_review_id",
                table: "book_review",
                newName: "IX_book_review_book_id");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    message_ = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    book_id = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("notification_pkey", x => x.notification_id);
                    table.ForeignKey(
                        name: "notification_book_id_fkey",
                        column: x => x.book_id,
                        principalTable: "book",
                        principalColumn: "book_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "notification_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "book",
                keyColumn: "book_id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "user",
                keyColumn: "user_id",
                keyValue: 1,
                columns: new[] { "created_at", "EmailConfirmed" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true });

            migrationBuilder.CreateIndex(
                name: "IX_chat_message_sender_id",
                table: "chat_message",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_book_id",
                table: "notification",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_user_id",
                table: "notification",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_image_avatar_id",
                table: "user",
                column: "avatar_id",
                principalTable: "image",
                principalColumn: "image_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_image_avatar_id",
                table: "user");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropIndex(
                name: "IX_chat_message_sender_id",
                table: "chat_message");

            migrationBuilder.RenameIndex(
                name: "IX_book_review_book_id",
                table: "book_review",
                newName: "idx_book_review_id");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "user",
                type: "boolean",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "api_token",
                table: "user",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "avg_rating",
                table: "user",
                type: "numeric(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "is_email_confirmed",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "user",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "book",
                keyColumn: "book_id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "user",
                keyColumn: "user_id",
                keyValue: 1,
                columns: new[] { "api_token", "avg_rating", "created_at", "EmailConfirmed", "is_email_confirmed", "updated_at" },
                values: new object[] { null, 0m, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, true, null });

            migrationBuilder.CreateIndex(
                name: "idx_user_review_owner",
                table: "user_review",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "user_api_token_key",
                table: "user",
                column: "api_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "user_email_key",
                table: "user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "role_role_name_key",
                table: "role",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_chat_conversation",
                table: "chat_message",
                columns: new[] { "sender_id", "receiver_id", "sent_at" });

            migrationBuilder.CreateIndex(
                name: "book_isbn_key",
                table: "book",
                column: "isbn",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "user_avatar_id_fkey",
                table: "user",
                column: "avatar_id",
                principalTable: "image",
                principalColumn: "image_id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
