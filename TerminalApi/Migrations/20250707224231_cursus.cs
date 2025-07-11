using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminalApi.Migrations
{
    /// <inheritdoc />
    public partial class cursus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("967063ef-31ff-4162-8ee7-6c0fea4e92e2"));

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Levels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Levels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cursus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cursus_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cursus_Levels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("5c68df4a-6be9-4291-95db-335e15e3c0ef"), 0.2m, new DateTime(2025, 7, 7, 22, 42, 31, 488, DateTimeKind.Utc).AddTicks(7749) });

            migrationBuilder.CreateIndex(
                name: "IX_Cursus_CategoryId",
                table: "Cursus",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Cursus_LevelId",
                table: "Cursus",
                column: "LevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cursus");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Levels");

            migrationBuilder.DeleteData(
                table: "TVARates",
                keyColumn: "Id",
                keyValue: new Guid("5c68df4a-6be9-4291-95db-335e15e3c0ef"));

            migrationBuilder.InsertData(
                table: "TVARates",
                columns: new[] { "Id", "Rate", "StartAt" },
                values: new object[] { new Guid("967063ef-31ff-4162-8ee7-6c0fea4e92e2"), 0.2m, new DateTime(2025, 7, 7, 21, 9, 15, 496, DateTimeKind.Utc).AddTicks(5312) });
        }
    }
}
