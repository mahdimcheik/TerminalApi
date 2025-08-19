using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class longaddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("8b358410-b208-4c0d-92ab-64000bce5f3f"));

            migrationBuilder.AlterColumn<string>(
                name: "StreetLine2",
                table: "Addresses",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Addresses",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("076b58b2-75f0-45b4-b0d7-efd629d0dc1d"), 0.2m, new DateTime(2025, 8, 19, 15, 49, 24, 128, DateTimeKind.Utc).AddTicks(7373) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("076b58b2-75f0-45b4-b0d7-efd629d0dc1d"));

            migrationBuilder.AlterColumn<string>(
                name: "StreetLine2",
                table: "Addresses",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Addresses",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("8b358410-b208-4c0d-92ab-64000bce5f3f"), 0.2m, new DateTime(2025, 8, 15, 7, 43, 44, 388, DateTimeKind.Utc).AddTicks(2940) });
        }
    }
}
