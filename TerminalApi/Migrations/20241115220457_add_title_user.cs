using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class add_title_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AspNetUsers",
                type: "Text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "AspNetUsers");
        }
    }
}
