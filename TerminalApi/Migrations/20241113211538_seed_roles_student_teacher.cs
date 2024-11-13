using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class seed_roles_student_teacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7f56db63-4e78-44a8-b681-ec1490a9b29d");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Discriminator", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "7f56db63-4e78-44a8-b681-ec1490a9b29s", "7f56db63-4e78-44a8-b681-ec1490a9b29s", "Role", "Student", "STUDENT" },
                    { "7f56db63-4e78-44a8-b681-ec1490a9b29T", "7f56db63-4e78-44a8-b681-ec1490a9b29d", "Role", "Teacher", "TEACHER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7f56db63-4e78-44a8-b681-ec1490a9b29s");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7f56db63-4e78-44a8-b681-ec1490a9b29T");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Discriminator", "Name", "NormalizedName" },
                values: new object[] { "7f56db63-4e78-44a8-b681-ec1490a9b29d", "7f56db63-4e78-44a8-b681-ec1490a9b29d", "Role", "SuperUser", "SUPERUSER" });
        }
    }
}
