using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.Models
{
    public enum DeliveryStatus
    {
        NotSent,
        Sent,
        Delivered,
        Failed
    }
    [Table("Attendees")]
    public class Attendee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool ChechkedIn { get; set; } = false;
        public DateTime? CheckedInTimestamp { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CustomId { get; set; }
        public string? Category { get; set; }
        public DeliveryStatus InvitationStatus { get; set; } = DeliveryStatus.NotSent;

        public int EventId { get; set; }
        public Event Event { get; set; }


    }
}