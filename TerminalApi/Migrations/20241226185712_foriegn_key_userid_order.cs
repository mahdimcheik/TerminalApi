﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class foriegn_key_userid_order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookerId",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BookerId",
                table: "Orders",
                column: "BookerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_BookerId",
                table: "Orders",
                column: "BookerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_BookerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BookerId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BookerId",
                table: "Orders");
        }
    }
}
