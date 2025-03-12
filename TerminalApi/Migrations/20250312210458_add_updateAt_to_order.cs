using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class add_updateAt_to_order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("1452068e-6693-4973-9ad2-e46cd830881f"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("37cbad9d-7ba4-4ed8-9d54-debb45fdb419"), 0.2m, new DateTime(2025, 3, 12, 21, 4, 58, 155, DateTimeKind.Utc).AddTicks(8392) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("37cbad9d-7ba4-4ed8-9d54-debb45fdb419"));

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Orders");

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("1452068e-6693-4973-9ad2-e46cd830881f"), 0.2m, new DateTime(2025, 3, 12, 9, 21, 17, 935, DateTimeKind.Utc).AddTicks(9954) });
        }
    }
}
