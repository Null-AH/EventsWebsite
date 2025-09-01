using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EventApi.Models;
using Newtonsoft.Json;

namespace EventApi.DTO.AttendeeDTO
{
    public class AddAttendeesRequestDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }
        [JsonPropertyName("customId")]
        public string? CustomId { get; set; }
        [JsonPropertyName("category")]
        public string? Category { get; set; }

    }
}