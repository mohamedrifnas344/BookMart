using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class refectorDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Descriptio",
                table: "Books",
                newName: "Description");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Books",
                newName: "Descriptio");
        }
    }
}
