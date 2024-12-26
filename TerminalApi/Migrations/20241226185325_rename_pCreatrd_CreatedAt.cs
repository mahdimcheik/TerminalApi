using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class rename_pCreatrd_CreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PCreatedAt",
                table: "Orders",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Orders",
                newName: "PCreatedAt");
        }
    }
}
