using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torath.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchasesAndPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPurchases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewspaperId = table.Column<int>(type: "int", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StripeSessionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPaymentComplete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPurchases", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "BaseContent",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 8, 8, 17, 24, 9, 612, DateTimeKind.Utc).AddTicks(7436));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPurchases");

            migrationBuilder.UpdateData(
                table: "BaseContent",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 8, 6, 20, 53, 7, 947, DateTimeKind.Utc).AddTicks(4594));
        }
    }
}
