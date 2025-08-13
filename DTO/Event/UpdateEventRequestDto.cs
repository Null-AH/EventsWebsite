using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EventApi.DTO.Event
{
    public class NotDefaultAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            var type = value.GetType();
            var defaultValue = Activator.CreateInstance(type);
            return !object.Equals(value, defaultValue);
        }
    }
    public class UpdateEventRequestDto
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(40, ErrorMessage = "Event name cannot be more than 40 letters")]
        [JsonPropertyName("eventName")]
        public string Name { get; set; } = string.Empty;
        [Required]
        [NotDefault(ErrorMessage = "Date is required.")]
        [JsonPropertyName("date")]
        public DateOnly EventDate { get; set; }
    }
}

