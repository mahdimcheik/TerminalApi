using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    public partial class Add_TVA_to_order : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("a1c4073b-27a5-4d8c-ae22-e129730db663"));

            migrationBuilder.AddColumn<decimal>(
                name: "TVARate",
                table: "Orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("2ba62d15-3f43-4164-9748-665a8d76a176"), 0.2m, new DateTime(2025, 3, 9, 16, 51, 8, 964, DateTimeKind.Utc).AddTicks(7146) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("2ba62d15-3f43-4164-9748-665a8d76a176"));

            migrationBuilder.DropColumn(
                name: "TVARate",
                table: "Orders");

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("a1c4073b-27a5-4d8c-ae22-e129730db663"), 0.2m, new DateTime(2025, 3, 9, 16, 30, 6, 643, DateTimeKind.Utc).AddTicks(837) });
        }
    }
}
