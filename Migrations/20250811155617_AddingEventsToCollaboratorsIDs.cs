using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class AddingEventsToCollaboratorsIDs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            INSERT INTO EventCollaborators (EventId, UserId, Role)
            SELECT 
                Id, 
                AppUserId, 
                0 
            FROM 
                Events
            WHERE 
                AppUserId IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DELETE FROM EventCollaborators 
            WHERE Role = 0 AND EventId IN (SELECT Id FROM Events WHERE AppUserId IS NOT NULL);
        ");
        }
    }
}
