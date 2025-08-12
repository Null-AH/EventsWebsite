using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EventApi.Models;

namespace EventApi.DTO.Event
{
    public class AddCollaboratorsRequestDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}