using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.DTO.Event
{
    public class EventCheckInResultDto
    {
        public string Status { get; set; }// Not Found, Success, AlreadyCheckedIn
        public string AttendeeName { get; set; }
        public int CheckedInCount { get; set; } 
    }
}