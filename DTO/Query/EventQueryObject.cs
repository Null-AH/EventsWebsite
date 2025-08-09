using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.DTO.Query
{
    public class EventQueryObject
    {

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 8;
        public bool IsDescending { get; set; } = false;
        public string OrderBy { get; set; } = "EventDate";
        public string? Name { get; set; }
    }
}