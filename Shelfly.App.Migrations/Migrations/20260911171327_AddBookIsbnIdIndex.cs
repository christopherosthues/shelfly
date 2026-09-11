using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shelfly.App.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddBookIsbnIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Books_ISBN_Id",
                table: "Books",
                columns: new[] { "ISBN", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_ISBN_Id",
                table: "Books");
        }
    }
}
