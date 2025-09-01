using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventApi.DTO.Internal
{
    public class WasenderWebHookMessageSentResponseDto
    { 
        [JsonPropertyName("event")]
        public string Event { get; set; }
        [JsonPropertyName("data")]
        public WasenderData Data { get; set; }
    }
        
    public class WasenderMessageSentData
    {
        [JsonPropertyName("key")]
        public WasenderKey Key { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
    public class WasenderMessageSentKey
    {
        [JsonPropertyName("id")]
        public string MessageId { get; set; }

        [JsonPropertyName("fromMe")]
        public bool FromMe { get; set; }
    }
}