using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventApi.DTO.Internal
{
    public class WasenderSendSuccessResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public WasenderResponseData Data { get; set; }
    }
    public class WasenderResponseData
    {
         [JsonPropertyName("msgId")]
        public long MessageId { get; set; }
    }
}