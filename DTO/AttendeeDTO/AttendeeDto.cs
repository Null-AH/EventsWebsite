using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.DTO.Attendee
{
    public class AttendeeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool ChechkedIn { get; set; } = false;
        public DateTime? CheckedInTimestamp { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CustomId { get; set; }
        public string? Category { get; set; }
        public string InvitationStatus { get; set; } = "Sent";

    }
}