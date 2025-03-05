using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gentlemen.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOutfitSlugNullable3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 5, 1, 6, 47, 833, DateTimeKind.Local).AddTicks(9420));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 5, 1, 6, 47, 833, DateTimeKind.Local).AddTicks(9429));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 5, 1, 6, 47, 833, DateTimeKind.Local).AddTicks(9430));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 5, 0, 58, 50, 381, DateTimeKind.Local).AddTicks(1806));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 5, 0, 58, 50, 381, DateTimeKind.Local).AddTicks(1815));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 3, 5, 0, 58, 50, 381, DateTimeKind.Local).AddTicks(1817));
        }
    }
}
