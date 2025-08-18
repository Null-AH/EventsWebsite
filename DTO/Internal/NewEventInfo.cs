using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.DTO.Event;
using EventApi.DTO.Template;
using EventApi.Models;

namespace EventApi.DTO.Internal
{
    public class NewEventInfo
    {
        public CreateEventDto EventDetails { get; set; }
        public List<TemplateElementsDto>? TemplateElements { get; set; }
        public IFormFile AttendeeFile { get; set; }
        public IFormFile? BackgroundImage { get; set; }
        public AppUser CreatingUser { get; set; } // We also need to know who is making the event
    }
}