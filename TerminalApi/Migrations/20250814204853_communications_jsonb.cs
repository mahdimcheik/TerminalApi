using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using TerminalApi.Models.Bookings;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class communications_jsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("1aa53bd9-8a1b-4b17-b2e4-bbc28ac5541f"));

            migrationBuilder.AddColumn<ICollection<ChatMessage>>(
                name: "Communications",
                table: "Bookings",
                type: "jsonb",
                nullable: true);

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("3172c0cf-8ca9-4cc2-b2b3-a7cabf017df7"), 0.2m, new DateTime(2025, 8, 14, 20, 48, 52, 834, DateTimeKind.Utc).AddTicks(4894) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("3172c0cf-8ca9-4cc2-b2b3-a7cabf017df7"));

            migrationBuilder.DropColumn(
                name: "Communications",
                table: "Bookings");

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("1aa53bd9-8a1b-4b17-b2e4-bbc28ac5541f"), 0.2m, new DateTime(2025, 7, 7, 22, 51, 31, 401, DateTimeKind.Utc).AddTicks(3100) });
        }
    }
}
