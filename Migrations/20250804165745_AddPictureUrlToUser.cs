using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPictureUrlToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "21d87ac1-e397-4ca5-8140-4b417f672b73");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5cbe6189-e633-4307-bb4d-3c5109b9fff4");

            migrationBuilder.AddColumn<string>(
                name: "PictureUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "d004a414-4ff2-422a-9f73-33e148799ea7", null, "Admin", "ADMIN" },
                    { "dbd32f6f-6e81-4930-9fda-80743f5d9166", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d004a414-4ff2-422a-9f73-33e148799ea7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "dbd32f6f-6e81-4930-9fda-80743f5d9166");

            migrationBuilder.DropColumn(
                name: "PictureUrl",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "21d87ac1-e397-4ca5-8140-4b417f672b73", null, "User", "USER" },
                    { "5cbe6189-e633-4307-bb4d-3c5109b9fff4", null, "Admin", "ADMIN" }
                });
        }
    }
}
