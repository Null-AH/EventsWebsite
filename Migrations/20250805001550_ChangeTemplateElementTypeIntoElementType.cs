using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTemplateElementTypeIntoElementType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2e2c3c6-e34a-43fe-b858-a04fe8a2161c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cd463057-ba5e-4069-b0f8-91817354df80");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "TemplateElements",
                newName: "ElementType");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3844ad92-cef9-4f36-bcb6-3c9ec56aa161", null, "User", "USER" },
                    { "d31661e6-29c7-4033-8e7b-700765abfb68", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3844ad92-cef9-4f36-bcb6-3c9ec56aa161");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d31661e6-29c7-4033-8e7b-700765abfb68");

            migrationBuilder.RenameColumn(
                name: "ElementType",
                table: "TemplateElements",
                newName: "Type");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "c2e2c3c6-e34a-43fe-b858-a04fe8a2161c", null, "Admin", "ADMIN" },
                    { "cd463057-ba5e-4069-b0f8-91817354df80", null, "User", "USER" }
                });
        }
    }
}
