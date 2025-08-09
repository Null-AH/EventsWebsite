using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTemplateElementsToDouble : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0d5dcda4-4e04-42d1-8bb8-07f45ade0b2f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "65619930-0bfe-42ee-8fef-b1862967f87c");

            migrationBuilder.AlterColumn<double>(
                name: "Y",
                table: "TemplateElements",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "X",
                table: "TemplateElements",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "Width",
                table: "TemplateElements",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "Height",
                table: "TemplateElements",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "c2e2c3c6-e34a-43fe-b858-a04fe8a2161c", null, "Admin", "ADMIN" },
                    { "cd463057-ba5e-4069-b0f8-91817354df80", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2e2c3c6-e34a-43fe-b858-a04fe8a2161c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cd463057-ba5e-4069-b0f8-91817354df80");

            migrationBuilder.AlterColumn<int>(
                name: "Y",
                table: "TemplateElements",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "X",
                table: "TemplateElements",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "Width",
                table: "TemplateElements",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "Height",
                table: "TemplateElements",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0d5dcda4-4e04-42d1-8bb8-07f45ade0b2f", null, "Admin", "ADMIN" },
                    { "65619930-0bfe-42ee-8fef-b1862967f87c", null, "User", "USER" }
                });
        }
    }
}
