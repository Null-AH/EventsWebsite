using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace EventApi.DTO.Internal
{
    public class WasenderWebHookResponseDto
    {
        [JsonPropertyName("event")]
        public string? Event { get; set; }
        [JsonPropertyName("data")]
        public WasenderData? Data { get; set; }
    }

    public class WasenderData
    {
        [JsonPropertyName("msgId")]
        public long? NumericMessageId { get; set; }
        [JsonPropertyName("id")]
        public string? AlphanumericId { get; set; }

        [JsonPropertyName("status")]
        public int? Status { get; set; }

        [JsonPropertyName("key")]
        public WasenderKey? Key { get; set; }
    }
    public class WasenderKey
    {
        [JsonPropertyName("id")]
        public string? MessageId { get; set; }

        [JsonPropertyName("fromMe")] 
        public bool FromMe { get; set; }
    }
}