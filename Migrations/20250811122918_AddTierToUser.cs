using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTierToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6fa88b80-8f88-4d14-9ee8-701370797362");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7f91046e-fad2-4746-bc53-c17b778549a4");

            migrationBuilder.AddColumn<int>(
                name: "Tier",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1865d0d9-7ac1-422d-95bb-8e1a34f8c062", null, "Admin", "ADMIN" },
                    { "3c0bacc4-9d3e-44d8-a995-5b75be32f346", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1865d0d9-7ac1-422d-95bb-8e1a34f8c062");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3c0bacc4-9d3e-44d8-a995-5b75be32f346");

            migrationBuilder.DropColumn(
                name: "Tier",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "6fa88b80-8f88-4d14-9ee8-701370797362", null, "Admin", "ADMIN" },
                    { "7f91046e-fad2-4746-bc53-c17b778549a4", null, "User", "USER" }
                });
        }
    }
}
