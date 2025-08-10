using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.DTO.Event
{
    public class EventSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly EventDate { get; set; }
        public int AttendeeCount { get; set; }
        public string? BackgroundImageUri { get; set; } = string.Empty;

    }
}