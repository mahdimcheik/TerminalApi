using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class retry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("3172c0cf-8ca9-4cc2-b2b3-a7cabf017df7"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("8b358410-b208-4c0d-92ab-64000bce5f3f"), 0.2m, new DateTime(2025, 8, 15, 7, 43, 44, 388, DateTimeKind.Utc).AddTicks(2940) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("8b358410-b208-4c0d-92ab-64000bce5f3f"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("3172c0cf-8ca9-4cc2-b2b3-a7cabf017df7"), 0.2m, new DateTime(2025, 8, 14, 20, 48, 52, 834, DateTimeKind.Utc).AddTicks(4894) });
        }
    }
}
