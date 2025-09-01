using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.DTO.Attendee;
using EventApi.DTO.AttendeeDTO;
using EventApi.Models;
using EventApi.Services;

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
                CheckedInTimestamp = a.CheckedInTimestamp,
                PhoneNumber = a.PhoneNumber,
                CustomId = a.CustomId,
                Category = a.Category,
                InvitationStatus = a.InvitationStatus.GetEnumMemberValue()
            }).ToList();
        }

        public static List<Attendee> AttendeesRequestDtoToAttendees(this List<AddAttendeesRequestDto> addAttendees,int eventId)
        {
            return addAttendees.Select(a => new Attendee
            {
                Name = a.Name,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                CustomId = a.CustomId,
                Category = a.Category,
                EventId = eventId
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