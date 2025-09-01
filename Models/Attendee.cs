using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace EventApi.Models
{
    public enum DeliveryStatus
    {
        [EnumMember(Value = "Not Sent")]
        NotSent,
        [EnumMember(Value = "Pending")]
        Pending,
        [EnumMember(Value = "Sent")]
        Sent,
        [EnumMember(Value = "Delivered")]
        Delivered,
        [EnumMember(Value = "Read")]
        Read,
        [EnumMember(Value = "Played")]
        Played
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
        public string? MessageId { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }


    }
}