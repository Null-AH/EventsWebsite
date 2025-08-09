using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedCreatedAtToEventModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "18037261-2fd8-45aa-80e9-9a2c0f43ca6d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "89d064c4-cdc6-4a90-bda6-06d1fcc6851c");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Events",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "acd49152-6dff-4e84-8e2a-a40b7344369a", null, "User", "USER" },
                    { "f0ea9460-698a-4f9f-b08b-1b3a5034c71f", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "acd49152-6dff-4e84-8e2a-a40b7344369a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f0ea9460-698a-4f9f-b08b-1b3a5034c71f");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Events");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "18037261-2fd8-45aa-80e9-9a2c0f43ca6d", null, "User", "USER" },
                    { "89d064c4-cdc6-4a90-bda6-06d1fcc6851c", null, "Admin", "ADMIN" }
                });
        }
    }
}
