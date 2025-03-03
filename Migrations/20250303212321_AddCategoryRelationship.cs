using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gentlemen.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StyleTips_Categories_CategoryId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_StyleTips_Categories_CategoryId",
                table: "StyleTips",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StyleTips_Categories_CategoryId",
                table: "StyleTips");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 3, 0, 52, 15, 704, DateTimeKind.Local).AddTicks(8160));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 3, 0, 52, 15, 704, DateTimeKind.Local).AddTicks(8195));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 3, 0, 52, 15, 704, DateTimeKind.Local).AddTicks(8199));

            migrationBuilder.AddForeignKey(
                name: "FK_StyleTips_Categories_CategoryId",
                table: "StyleTips",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }
    }
}
