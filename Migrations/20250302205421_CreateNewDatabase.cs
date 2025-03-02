using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gentlemen.Migrations
{
    /// <inheritdoc />
    public partial class CreateNewDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RedirectUrl",
                table: "FeaturedCategories",
                newName: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Category",
                table: "FeaturedCategories",
                newName: "RedirectUrl");
        }
    }
}
