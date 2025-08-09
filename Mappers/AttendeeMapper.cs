using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.DTO.Attendee;
using EventApi.Models;

namespace EventApi.Mappers
{
    public static class AttendeeMapper
    {
        public static List<AttendeeDto> AttendeeToDto(this List<Attendee> attendeeModel)
        {
            return attendeeModel.Select(a => new AttendeeDto
            {
                Id = a.Id,
                Name = a.Name,
                Email = a.Email,
                ChechkedIn = a.ChechkedIn,
                CheckedInTimestamp = a.CheckedInTimestamp
            }).ToList();
        }
        
        public static List<AttendeeDto> AttendeeToEssentialsDto(this List<Attendee> attendeeModel)
        {
            return attendeeModel.Select(a => new AttendeeDto
            {
                Name = a.Name,
                Email = a.Email,
            }).ToList();
        }
        
    }
}