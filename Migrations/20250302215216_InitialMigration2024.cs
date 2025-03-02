using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gentlemen.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration2024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeaturedCategories");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "StyleTips",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Slug = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "ImageUrl", "IsActive", "Slug", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 3, 3, 0, 52, 15, 704, DateTimeKind.Local).AddTicks(8160), "Profesyonel ve şık görünüm için öneriler", "/images/business-style.jpg", true, "is-stili", "İş Stili" },
                    { 2, new DateTime(2025, 3, 3, 0, 52, 15, 704, DateTimeKind.Local).AddTicks(8195), "Rahat ve trend günlük kombinler", "/images/casual-style.jpg", true, "gunluk-stil", "Günlük Stil" },
                    { 3, new DateTime(2025, 3, 3, 0, 52, 15, 704, DateTimeKind.Local).AddTicks(8199), "Özel anlar için şık seçimler", "/images/special-occasions.jpg", true, "ozel-gunler", "Özel Günler" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_StyleTips_CategoryId",
                table: "StyleTips",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_StyleTips_Categories_CategoryId",
                table: "StyleTips",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StyleTips_Categories_CategoryId",
                table: "StyleTips");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_StyleTips_CategoryId",
                table: "StyleTips");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "StyleTips");

            migrationBuilder.CreateTable(
                name: "FeaturedCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Category = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturedCategories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
