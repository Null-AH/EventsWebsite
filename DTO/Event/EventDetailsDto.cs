using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.DTO.Attendee;
using EventApi.DTO.Template;
using EventApi.Models;

namespace EventApi.DTO.Event
{
    public class EventDetailsDto
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public DateOnly EventDate { get; set; }
        public string? Location { get; set; }
        public string? BackgroundImageUri { get; set; }
        public string? GeneratedInvitationFullPath { get; set; }
        public List<AttendeeDto> Attendees { get; set; } = new List<AttendeeDto>();
        public List<TemplateElementsDto> TemplateElements { get; set; } = new List<TemplateElementsDto>();
    }
}