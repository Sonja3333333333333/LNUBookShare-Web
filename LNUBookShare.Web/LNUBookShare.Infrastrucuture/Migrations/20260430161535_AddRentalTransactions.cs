using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LNUBookShare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rental_transaction",
                columns: table => new
                {
                    transaction_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    book_id = table.Column<int>(type: "integer", nullable: false),
                    owner_id = table.Column<int>(type: "integer", nullable: false),
                    borrower_id = table.Column<int>(type: "integer", nullable: false),
                    issue_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expected_return_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    actual_return_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'active'::character varying"),
                },
                constraints: table =>
                {
                    table.PrimaryKey("rental_transaction_pkey", x => x.transaction_id);
                    table.ForeignKey(
                        name: "rental_transaction_book_id_fkey",
                        column: x => x.book_id,
                        principalTable: "book",
                        principalColumn: "book_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "rental_transaction_borrower_id_fkey",
                        column: x => x.borrower_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "rental_transaction_owner_id_fkey",
                        column: x => x.owner_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rental_transaction_book_id",
                table: "rental_transaction",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "IX_rental_transaction_borrower_id",
                table: "rental_transaction",
                column: "borrower_id");

            migrationBuilder.CreateIndex(
                name: "IX_rental_transaction_owner_id",
                table: "rental_transaction",
                column: "owner_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rental_transaction");
        }
    }
}
