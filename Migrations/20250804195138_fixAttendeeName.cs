using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class fixAttendeeName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "371a005e-2b90-4152-ad32-2b354850d9c8");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3c1dc986-0d84-4052-ac33-a91f0e8ecfdd");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2b438b64-8b40-43be-b8d5-f54d6b1fff26", null, "User", "USER" },
                    { "ad1c5aec-5a54-44f6-9f09-08faaf1d6569", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2b438b64-8b40-43be-b8d5-f54d6b1fff26");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ad1c5aec-5a54-44f6-9f09-08faaf1d6569");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "371a005e-2b90-4152-ad32-2b354850d9c8", null, "User", "USER" },
                    { "3c1dc986-0d84-4052-ac33-a91f0e8ecfdd", null, "Admin", "ADMIN" }
                });
        }
    }
}
