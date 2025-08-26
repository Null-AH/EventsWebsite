using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedPhoneCustomIdCatgoryInvitationStatusFieldsToAttendees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Attendees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomId",
                table: "Attendees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvitationStatus",
                table: "Attendees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Attendees",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Attendees");

            migrationBuilder.DropColumn(
                name: "CustomId",
                table: "Attendees");

            migrationBuilder.DropColumn(
                name: "InvitationStatus",
                table: "Attendees");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Attendees");
        }
    }
}
