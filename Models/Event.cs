using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.Models
{

    [Table("Events")] // Good practice to explicitly name the table
    public class Event
    {
        public int Id { get; set; } 

        public string Name { get; set; } = string.Empty; 
        
        public DateTime EventDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? Location { get; set; } 

        public string? Description { get; set; }
        public string? EventImageUri { get; set; }

        public string? BackgroundImageUri { get; set; }
        public string? GeneratedInvitationsZipUri { get; set; }


        public string AppUserId { get; set; } 

        public AppUser AppUser { get; set; }

        public List<Attendee> Attendees { get; set; } = new List<Attendee>();
        public List<TemplateElement> TemplateElements { get; set; } = new List<TemplateElement>();
    }
}
