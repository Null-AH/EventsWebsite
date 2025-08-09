using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class AddingFirebaseAuthId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "04fa8039-b0dc-4db3-a541-4b94a0abb962");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c0fb6e64-a0a6-4a72-b3c9-f50a4308043c");

            migrationBuilder.AddColumn<string>(
                name: "FirebaseUid",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "21d87ac1-e397-4ca5-8140-4b417f672b73", null, "User", "USER" },
                    { "5cbe6189-e633-4307-bb4d-3c5109b9fff4", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "21d87ac1-e397-4ca5-8140-4b417f672b73");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5cbe6189-e633-4307-bb4d-3c5109b9fff4");

            migrationBuilder.DropColumn(
                name: "FirebaseUid",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "04fa8039-b0dc-4db3-a541-4b94a0abb962", null, "Admin", "ADMIN" },
                    { "c0fb6e64-a0a6-4a72-b3c9-f50a4308043c", null, "User", "USER" }
                });
        }
    }
}
