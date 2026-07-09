using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torath.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Magazines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ISSN = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Magazines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Magazines_BaseContent_Id",
                        column: x => x.Id,
                        principalTable: "BaseContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Newspapers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Newspapers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Newspapers_BaseContent_Id",
                        column: x => x.Id,
                        principalTable: "BaseContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResearchPapers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Authors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Abstract = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublicationYear = table.Column<int>(type: "int", nullable: false),
                    JournalOrConferenceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DOI = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchPapers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchPapers_BaseContent_Id",
                        column: x => x.Id,
                        principalTable: "BaseContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MagazineIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VolumeNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MagazineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MagazineIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MagazineIssues_Magazines_MagazineId",
                        column: x => x.MagazineId,
                        principalTable: "Magazines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewspaperIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NewspaperId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewspaperIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewspaperIssues_Newspapers_NewspaperId",
                        column: x => x.NewspaperId,
                        principalTable: "Newspapers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MagazineIssueId = table.Column<int>(type: "int", nullable: true),
                    NewspaperIssueId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Articles_MagazineIssues_MagazineIssueId",
                        column: x => x.MagazineIssueId,
                        principalTable: "MagazineIssues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Articles_NewspaperIssues_NewspaperIssueId",
                        column: x => x.NewspaperIssueId,
                        principalTable: "NewspaperIssues",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_MagazineIssueId",
                table: "Articles",
                column: "MagazineIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_NewspaperIssueId",
                table: "Articles",
                column: "NewspaperIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineIssues_MagazineId",
                table: "MagazineIssues",
                column: "MagazineId");

            migrationBuilder.CreateIndex(
                name: "IX_NewspaperIssues_NewspaperId",
                table: "NewspaperIssues",
                column: "NewspaperId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "ResearchPapers");

            migrationBuilder.DropTable(
                name: "MagazineIssues");

            migrationBuilder.DropTable(
                name: "NewspaperIssues");

            migrationBuilder.DropTable(
                name: "Magazines");

            migrationBuilder.DropTable(
                name: "Newspapers");
        }
    }
}
