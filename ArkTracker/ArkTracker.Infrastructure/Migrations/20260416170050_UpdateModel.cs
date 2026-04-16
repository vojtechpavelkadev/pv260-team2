using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArkTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "IngestedAtUtc",
                table: "Holdings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Holdings_IngestedAtUtc",
                table: "Holdings",
                column: "IngestedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Holdings_IngestedAtUtc",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "IngestedAtUtc",
                table: "Holdings");
        }
    }
}
