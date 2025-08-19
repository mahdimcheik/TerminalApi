using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class longaddressremovelength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("076b58b2-75f0-45b4-b0d7-efd629d0dc1d"));

            migrationBuilder.AlterColumn<string>(
                name: "StreetLine2",
                table: "Addresses",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Addresses",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("712ad802-a9f9-41d9-a33a-9501af72bb3a"), 0.2m, new DateTime(2025, 8, 19, 15, 51, 55, 492, DateTimeKind.Utc).AddTicks(976) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("712ad802-a9f9-41d9-a33a-9501af72bb3a"));

            migrationBuilder.AlterColumn<string>(
                name: "StreetLine2",
                table: "Addresses",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Addresses",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("076b58b2-75f0-45b4-b0d7-efd629d0dc1d"), 0.2m, new DateTime(2025, 8, 19, 15, 49, 24, 128, DateTimeKind.Utc).AddTicks(7373) });
        }
    }
}
