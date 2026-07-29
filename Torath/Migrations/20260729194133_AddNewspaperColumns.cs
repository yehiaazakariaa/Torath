using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torath.Migrations
{
    /// <inheritdoc />
    public partial class AddNewspaperColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "Newspapers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PdfFilePath",
                table: "Newspapers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Newspapers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "BaseContent",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 7, 29, 19, 41, 33, 46, DateTimeKind.Utc).AddTicks(3669));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "Newspapers");

            migrationBuilder.DropColumn(
                name: "PdfFilePath",
                table: "Newspapers");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Newspapers");

            migrationBuilder.UpdateData(
                table: "BaseContent",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 7, 11, 2, 39, 35, 123, DateTimeKind.Utc).AddTicks(8796));
        }
    }
}
