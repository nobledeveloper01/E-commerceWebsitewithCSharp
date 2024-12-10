using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_commerceWebsite.Migrations
{
    /// <inheritdoc />
    public partial class ContactFormTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DateRegistered", "PasswordHash" },
                values: new object[] { new DateTime(2024, 12, 6, 17, 41, 25, 29, DateTimeKind.Utc).AddTicks(3718), "AQAAAAIAAYagAAAAEOpB2HB9rCTSMv44FXjnmowKnVfiAfok7dhj+0xNdxZTr8Ojchiwk02dY4dvXY2z1A==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactMessages");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DateRegistered", "PasswordHash" },
                values: new object[] { new DateTime(2024, 12, 4, 15, 24, 25, 52, DateTimeKind.Utc).AddTicks(6036), "AQAAAAIAAYagAAAAEI5PISPOjSjqwhAGjH5xGy4hHGfeMGpRXRmMrvya51LtuQL/dxEFgeGs+2R8skRT2Q==" });
        }
    }
}
