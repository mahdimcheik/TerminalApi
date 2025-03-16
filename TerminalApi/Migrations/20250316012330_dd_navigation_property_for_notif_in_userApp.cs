using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class dd_navigation_property_for_notif_in_userApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("3c96bdfe-2181-4cd8-8510-d1f81e9bca1b"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("82c7bf30-292b-46ac-8e26-40208dce7da7"), 0.2m, new DateTime(2025, 3, 16, 1, 23, 29, 616, DateTimeKind.Utc).AddTicks(5137) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("82c7bf30-292b-46ac-8e26-40208dce7da7"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("3c96bdfe-2181-4cd8-8510-d1f81e9bca1b"), 0.2m, new DateTime(2025, 3, 12, 23, 33, 36, 815, DateTimeKind.Utc).AddTicks(8915) });
        }
    }
}
