using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class add_paymentIntent_to_order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("2ba62d15-3f43-4164-9748-665a8d76a176"));

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntent",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("1452068e-6693-4973-9ad2-e46cd830881f"), 0.2m, new DateTime(2025, 3, 12, 9, 21, 17, 935, DateTimeKind.Utc).AddTicks(9954) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("1452068e-6693-4973-9ad2-e46cd830881f"));

            migrationBuilder.DropColumn(
                name: "PaymentIntent",
                table: "Orders");

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("2ba62d15-3f43-4164-9748-665a8d76a176"), 0.2m, new DateTime(2025, 3, 9, 16, 51, 8, 964, DateTimeKind.Utc).AddTicks(7146) });
        }
    }
}
