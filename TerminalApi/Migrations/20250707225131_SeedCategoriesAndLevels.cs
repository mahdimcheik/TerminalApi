using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategoriesAndLevels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("5c68df4a-6be9-4291-95db-335e15e3c0ef"));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Color", "Icon", "Name" },
                values: new object[,]
                {
                    { new Guid("10101010-1010-1010-1010-101010101010"), "#6366f1", "🤖", "Intelligence Artificielle" },
                    { new Guid("20202020-2020-2020-2020-202020202020"), "#f59e0b", "⚙️", "DevOps" },
                    { new Guid("30303030-3030-3030-3030-303030303030"), "#10b981", "📱", "Mobile" },
                    { new Guid("40404040-4040-4040-4040-404040404040"), "#64748b", "📋", "Gestion de projet" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "#3b82f6", "💻", "Programmation" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "#ec4899", "🎨", "Design" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "#059669", "📊", "Business" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "#dc2626", "📢", "Marketing" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "#7c3aed", "📈", "Data Science" },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "#1f2937", "🔒", "Cybersécurité" }
                });

            migrationBuilder.InsertData(
                table: "Levels",
                columns: new[] { "Id", "Color", "Icon", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "#22c55e", "🟢", "Débutant" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "#eab308", "🟡", "Intermédiaire" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "#f97316", "🟠", "Avancé" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "#ef4444", "🔴", "Expert" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "#8b5cf6", "⭐", "Tous niveaux" }
                });

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("1aa53bd9-8a1b-4b17-b2e4-bbc28ac5541f"), 0.2m, new DateTime(2025, 7, 7, 22, 51, 31, 401, DateTimeKind.Utc).AddTicks(3100) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("10101010-1010-1010-1010-101010101010"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("30303030-3030-3030-3030-303030303030"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("40404040-4040-4040-4040-404040404040"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"));

            migrationBuilder.DeleteData(
                table: "Levels",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Levels",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Levels",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Levels",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Levels",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("1aa53bd9-8a1b-4b17-b2e4-bbc28ac5541f"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("5c68df4a-6be9-4291-95db-335e15e3c0ef"), 0.2m, new DateTime(2025, 7, 7, 22, 42, 31, 488, DateTimeKind.Utc).AddTicks(7749) });
        }
    }
}
