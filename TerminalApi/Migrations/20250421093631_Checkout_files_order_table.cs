using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class Checkout_files_order_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("097ddc18-1016-4c05-a823-867a41c99574"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckoutExpiredAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutID",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("71f15758-4b4a-42d5-812d-d363fc3b1d1e"), 0.2m, new DateTime(2025, 4, 21, 9, 36, 31, 191, DateTimeKind.Utc).AddTicks(8991) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("71f15758-4b4a-42d5-812d-d363fc3b1d1e"));

            migrationBuilder.DropColumn(
                name: "CheckoutExpiredAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CheckoutID",
                table: "Orders");

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("097ddc18-1016-4c05-a823-867a41c99574"), 0.2m, new DateTime(2025, 4, 19, 9, 49, 53, 776, DateTimeKind.Utc).AddTicks(7880) });
        }
    }
}
