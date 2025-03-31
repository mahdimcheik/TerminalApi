using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class fix_notificationDatetime_formatv1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("f715843e-4e49-438d-9d37-e3ca0f2fdbc2"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("3ac0b7b8-292e-4497-8c39-dd7aea95458d"), 0.2m, new DateTime(2025, 3, 30, 19, 42, 40, 913, DateTimeKind.Utc).AddTicks(6686) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("3ac0b7b8-292e-4497-8c39-dd7aea95458d"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("f715843e-4e49-438d-9d37-e3ca0f2fdbc2"), 0.2m, new DateTime(2025, 3, 30, 19, 38, 32, 185, DateTimeKind.Utc).AddTicks(9620) });
        }
    }
}
