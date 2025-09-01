using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.Models;

namespace EventApi.DTO.AttendeeDTO
{
    public class EditAttendeesRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool ChechkedIn { get; set; } = false;
        public string? PhoneNumber { get; set; }
        public string? CustomId { get; set; }
        public string? Category { get; set; }
        public string? InvitationStatus { get; set; }

    }
}