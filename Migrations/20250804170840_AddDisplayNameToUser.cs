using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNameToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d004a414-4ff2-422a-9f73-33e148799ea7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "dbd32f6f-6e81-4930-9fda-80743f5d9166");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "46c2e229-b6c0-4afc-a0ed-ade5216cea46", null, "User", "USER" },
                    { "c22717cb-9a22-4f7a-b867-b6c6e454908f", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "46c2e229-b6c0-4afc-a0ed-ade5216cea46");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c22717cb-9a22-4f7a-b867-b6c6e454908f");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "d004a414-4ff2-422a-9f73-33e148799ea7", null, "Admin", "ADMIN" },
                    { "dbd32f6f-6e81-4930-9fda-80743f5d9166", null, "User", "USER" }
                });
        }
    }
}
