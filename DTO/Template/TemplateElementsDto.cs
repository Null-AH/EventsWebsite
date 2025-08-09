using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventApi.DTO.Template
{
    public class TemplateElementsDto
    {
        [JsonPropertyName("type")]
        public string ElementType { get; set; } = string.Empty;
        [JsonPropertyName("x")]
        public double X { get; set; }
        [JsonPropertyName("y")]
        public double Y { get; set; }
        [JsonPropertyName("width")]
        public double Width { get; set; }
        [JsonPropertyName("height")]
        public double Height { get; set; }
        [JsonPropertyName("fontColor")]
        public string? FontColor { get; set; }
        public string? FontTheme { get; set; }
    }
}