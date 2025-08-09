using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class AddingDecriptionAndImageToEventModelAndModifyingInvitationsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3844ad92-cef9-4f36-bcb6-3c9ec56aa161");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d31661e6-29c7-4033-8e7b-700765abfb68");

            migrationBuilder.DropColumn(
                name: "CheckedInTimestamp",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "GeneratedImageURI",
                table: "Invitations");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventImageUri",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedInvitationsZipUri",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "18037261-2fd8-45aa-80e9-9a2c0f43ca6d", null, "User", "USER" },
                    { "89d064c4-cdc6-4a90-bda6-06d1fcc6851c", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "18037261-2fd8-45aa-80e9-9a2c0f43ca6d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "89d064c4-cdc6-4a90-bda6-06d1fcc6851c");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventImageUri",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "GeneratedInvitationsZipUri",
                table: "Events");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInTimestamp",
                table: "Invitations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedImageURI",
                table: "Invitations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3844ad92-cef9-4f36-bcb6-3c9ec56aa161", null, "User", "USER" },
                    { "d31661e6-29c7-4033-8e7b-700765abfb68", null, "Admin", "ADMIN" }
                });
        }
    }
}
