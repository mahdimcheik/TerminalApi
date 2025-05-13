using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class orders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("71f15758-4b4a-42d5-812d-d363fc3b1d1e"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("66cf7f58-cf6d-4dcc-a8ca-aebb28fc21c6"), 0.2m, new DateTime(2025, 5, 13, 8, 42, 10, 856, DateTimeKind.Utc).AddTicks(7743) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("66cf7f58-cf6d-4dcc-a8ca-aebb28fc21c6"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("71f15758-4b4a-42d5-812d-d363fc3b1d1e"), 0.2m, new DateTime(2025, 4, 21, 9, 36, 31, 191, DateTimeKind.Utc).AddTicks(8991) });
        }
    }
}
