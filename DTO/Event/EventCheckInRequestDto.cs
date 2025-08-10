using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.DTO.Event
{
    public class EventCheckInRequestDto
    {
        [Required]
        public string email { get; set; } = string.Empty;
    }
}