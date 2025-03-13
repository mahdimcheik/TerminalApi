using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class modifying__notification_sender_reciever_createAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_StudentId",
                table: "Notifications");

            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("fc289aaf-72a3-4e1e-895e-167e00f6e15b"));

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Notifications",
                newName: "SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_StudentId",
                table: "Notifications",
                newName: "IX_Notifications_SenderId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RecipientId",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("3c96bdfe-2181-4cd8-8510-d1f81e9bca1b"), 0.2m, new DateTime(2025, 3, 12, 23, 33, 36, 815, DateTimeKind.Utc).AddTicks(8915) });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_RecipientId",
                table: "Notifications",
                column: "RecipientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_SenderId",
                table: "Notifications",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_RecipientId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_SenderId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications");

            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("3c96bdfe-2181-4cd8-8510-d1f81e9bca1b"));

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RecipientId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "Notifications",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_SenderId",
                table: "Notifications",
                newName: "IX_Notifications_StudentId");

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("fc289aaf-72a3-4e1e-895e-167e00f6e15b"), 0.2m, new DateTime(2025, 3, 12, 23, 1, 29, 635, DateTimeKind.Utc).AddTicks(1905) });

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_StudentId",
                table: "Notifications",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
