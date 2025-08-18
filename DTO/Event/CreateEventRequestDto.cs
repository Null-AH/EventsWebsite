using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.DTO.Event
{
    public class CreateEventRequestDto
    {
        public string EventInfo { get; set; }
        public string? TemplateInfo { get; set; }
        public IFormFile AttendeeFile { get; set; }
        public IFormFile? BackgroundImage { get; set; }        
    }
}