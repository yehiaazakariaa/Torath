using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torath.Migrations
{
    /// <inheritdoc />
    public partial class DisableIdentityCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tell EF not to wrap this specific command in a transaction
            migrationBuilder.Sql("ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = OFF;", suppressTransaction: true);

            migrationBuilder.UpdateData(
                table: "BaseContent",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 7, 30, 12, 38, 47, 109, DateTimeKind.Utc).AddTicks(9984));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Tell EF not to wrap this specific command in a transaction
            migrationBuilder.Sql("ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = ON;", suppressTransaction: true);

            migrationBuilder.UpdateData(
                table: "BaseContent",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 7, 29, 19, 41, 33, 46, DateTimeKind.Utc).AddTicks(3669));
        }
    }
}