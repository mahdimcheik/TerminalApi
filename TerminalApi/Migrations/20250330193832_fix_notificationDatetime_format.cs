using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class fix_notificationDatetime_format : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("82c7bf30-292b-46ac-8e26-40208dce7da7"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("f715843e-4e49-438d-9d37-e3ca0f2fdbc2"), 0.2m, new DateTime(2025, 3, 30, 19, 38, 32, 185, DateTimeKind.Utc).AddTicks(9620) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("f715843e-4e49-438d-9d37-e3ca0f2fdbc2"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("82c7bf30-292b-46ac-8e26-40208dce7da7"), 0.2m, new DateTime(2025, 3, 16, 1, 23, 29, 616, DateTimeKind.Utc).AddTicks(5137) });
        }
    }
}
