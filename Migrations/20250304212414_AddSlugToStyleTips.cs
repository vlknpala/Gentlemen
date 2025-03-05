using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gentlemen.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToStyleTips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "StyleTips",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 5, 0, 24, 13, 692, DateTimeKind.Local).AddTicks(3118));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 5, 0, 24, 13, 692, DateTimeKind.Local).AddTicks(3128));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 5, 0, 24, 13, 692, DateTimeKind.Local).AddTicks(3129));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "StyleTips");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 4, 0, 23, 21, 364, DateTimeKind.Local).AddTicks(5945));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 4, 0, 23, 21, 364, DateTimeKind.Local).AddTicks(5955));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 4, 0, 23, 21, 364, DateTimeKind.Local).AddTicks(5956));
        }
    }
}
