using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventApi.DTO.Event
{
    public class CreateEventDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("eventDate")]
        public DateOnly EventDate { get; set; }

        public string? Location { get; set; } 

        public string? BackgroundImageUri { get; set; }
    }
}