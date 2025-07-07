using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class isbanned_field_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("bfdb1dc5-c57d-4cbb-b0e9-9fcaebb8ef92"));

            migrationBuilder.AddColumn<DateTime>(
                name: "BannedUntilDate",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBanned",
                table: "AspNetUsers",
                type: "boolean",
                nullable: true);

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("097ddc18-1016-4c05-a823-867a41c99574"), 0.2m, new DateTime(2025, 4, 19, 9, 49, 53, 776, DateTimeKind.Utc).AddTicks(7880) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("097ddc18-1016-4c05-a823-867a41c99574"));

            migrationBuilder.DropColumn(
                name: "BannedUntilDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsBanned",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("bfdb1dc5-c57d-4cbb-b0e9-9fcaebb8ef92"), 0.2m, new DateTime(2025, 4, 18, 20, 23, 57, 136, DateTimeKind.Utc).AddTicks(4171) });
        }
    }
}
