using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameAttendeeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Atفendees_Events_EventId",
                table: "Atفendees");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Atفendees_AttendeeId",
                table: "Invitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Atفendees",
                table: "Atفendees");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2b438b64-8b40-43be-b8d5-f54d6b1fff26");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ad1c5aec-5a54-44f6-9f09-08faaf1d6569");

            migrationBuilder.RenameTable(
                name: "Atفendees",
                newName: "Attendees");

            migrationBuilder.RenameIndex(
                name: "IX_Atفendees_EventId",
                table: "Attendees",
                newName: "IX_Attendees_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attendees",
                table: "Attendees",
                column: "Id");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0d5dcda4-4e04-42d1-8bb8-07f45ade0b2f", null, "Admin", "ADMIN" },
                    { "65619930-0bfe-42ee-8fef-b1862967f87c", null, "User", "USER" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Attendees_Events_EventId",
                table: "Attendees",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Attendees_AttendeeId",
                table: "Invitations",
                column: "AttendeeId",
                principalTable: "Attendees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendees_Events_EventId",
                table: "Attendees");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Attendees_AttendeeId",
                table: "Invitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attendees",
                table: "Attendees");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0d5dcda4-4e04-42d1-8bb8-07f45ade0b2f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "65619930-0bfe-42ee-8fef-b1862967f87c");

            migrationBuilder.RenameTable(
                name: "Attendees",
                newName: "Atفendees");

            migrationBuilder.RenameIndex(
                name: "IX_Attendees_EventId",
                table: "Atفendees",
                newName: "IX_Atفendees_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Atفendees",
                table: "Atفendees",
                column: "Id");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2b438b64-8b40-43be-b8d5-f54d6b1fff26", null, "User", "USER" },
                    { "ad1c5aec-5a54-44f6-9f09-08faaf1d6569", null, "Admin", "ADMIN" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Atفendees_Events_EventId",
                table: "Atفendees",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Atفendees_AttendeeId",
                table: "Invitations",
                column: "AttendeeId",
                principalTable: "Atفendees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
