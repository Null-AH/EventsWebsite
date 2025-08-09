using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class AddingEventAndOtherModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "46c2e229-b6c0-4afc-a0ed-ade5216cea46");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c22717cb-9a22-4f7a-b867-b6c6e454908f");

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BackgroundImageUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Atفendees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChechkedIn = table.Column<bool>(type: "bit", nullable: false),
                    CheckedInTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atفendees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Atفendees_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateElements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    FontColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FontTheme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateElements_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueQRCode = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneratedImageURI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckedInTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttendeeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invitations_Atفendees_AttendeeId",
                        column: x => x.AttendeeId,
                        principalTable: "Atفendees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "371a005e-2b90-4152-ad32-2b354850d9c8", null, "User", "USER" },
                    { "3c1dc986-0d84-4052-ac33-a91f0e8ecfdd", null, "Admin", "ADMIN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Atفendees_EventId",
                table: "Atفendees",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_AppUserId",
                table: "Events",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_AttendeeId",
                table: "Invitations",
                column: "AttendeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateElements_EventId",
                table: "TemplateElements",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invitations");

            migrationBuilder.DropTable(
                name: "TemplateElements");

            migrationBuilder.DropTable(
                name: "Atفendees");

            migrationBuilder.DropTable(
                name: "Events");

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
                    { "46c2e229-b6c0-4afc-a0ed-ade5216cea46", null, "User", "USER" },
                    { "c22717cb-9a22-4f7a-b867-b6c6e454908f", null, "Admin", "ADMIN" }
                });
        }
    }
}
