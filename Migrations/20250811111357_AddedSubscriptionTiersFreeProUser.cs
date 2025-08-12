using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedSubscriptionTiersFreeProUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b42ec9a7-a73c-4f2f-9aaf-ce316b9a3a88");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d45556d3-73ac-41c5-8432-ab968deb39e9");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "6fa88b80-8f88-4d14-9ee8-701370797362", null, "Admin", "ADMIN" },
                    { "7f91046e-fad2-4746-bc53-c17b778549a4", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6fa88b80-8f88-4d14-9ee8-701370797362");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7f91046e-fad2-4746-bc53-c17b778549a4");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "b42ec9a7-a73c-4f2f-9aaf-ce316b9a3a88", null, "User", "USER" },
                    { "d45556d3-73ac-41c5-8432-ab968deb39e9", null, "Admin", "ADMIN" }
                });
        }
    }
}
