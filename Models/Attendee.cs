using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.Models
{
    [Table("Attendees")]
    public class Attendee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool ChechkedIn { get; set; } = false;
        public DateTime? CheckedInTimestamp { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        
    }
}