using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventApi.DTO.Event
{
    public class EditCollaboratorRequestDto
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}