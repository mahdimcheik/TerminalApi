using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class add_navigations_properties_toUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("44d9e251-7b34-491c-9184-d5069ba32077"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("fc289aaf-72a3-4e1e-895e-167e00f6e15b"), 0.2m, new DateTime(2025, 3, 12, 23, 1, 29, 635, DateTimeKind.Utc).AddTicks(1905) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("fc289aaf-72a3-4e1e-895e-167e00f6e15b"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("44d9e251-7b34-491c-9184-d5069ba32077"), 0.2m, new DateTime(2025, 3, 12, 22, 57, 24, 333, DateTimeKind.Utc).AddTicks(9759) });
        }
    }
}
