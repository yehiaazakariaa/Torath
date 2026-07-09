using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torath.Migrations
{
    /// <inheritdoc />
    public partial class AddBookSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BaseContent",
                columns: new[] { "Id", "CategoryId", "CreatedDate", "Description", "Language", "PublicationDate", "Publisher", "Title", "UpdatedDate" },
                values: new object[] { 1, 1, new DateTime(2026, 7, 9, 3, 48, 55, 437, DateTimeKind.Utc).AddTicks(1811), "A Craftsman's Guide to Software Structure", "English", new DateTime(2017, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Prentice Hall", "Clean Architecture", null });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Authors", "Edition", "ISBN", "NumberOfPages" },
                values: new object[] { 1, "Robert C. Martin", "1st", "978-0134494166", 432 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BaseContent",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
