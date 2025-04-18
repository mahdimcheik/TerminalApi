using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class remove_refreshToken_from_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("fb1eda0c-5643-4fc5-a549-c3dc1a172ad4"));

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("bfdb1dc5-c57d-4cbb-b0e9-9fcaebb8ef92"), 0.2m, new DateTime(2025, 4, 18, 20, 23, 57, 136, DateTimeKind.Utc).AddTicks(4171) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("bfdb1dc5-c57d-4cbb-b0e9-9fcaebb8ef92"));

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("fb1eda0c-5643-4fc5-a549-c3dc1a172ad4"), 0.2m, new DateTime(2025, 4, 12, 13, 49, 6, 782, DateTimeKind.Utc).AddTicks(7266) });
        }
    }
}
