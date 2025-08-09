using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.DTO.Event;
using EventApi.Migrations;
using EventApi.Models;
using NPOI.SS.Formula.Functions;

namespace EventApi.Mappers
{
    public static class EventMapper
    {
        public static Event ToEventFromEventRequestDto(this CreateEventDto eventDto, AppUser creatingUser, string backgroundImage)
        {
            return new Event
            {
                Name = eventDto.Name,
                EventDate = eventDto.EventDate,
                Location = eventDto.Location,
                BackgroundImageUri = backgroundImage,
                AppUserId = creatingUser.Id
            };
        }

        public static EventSummaryDto EventToSummaryDto(this Event eventModel, int attendeesCount)
        {
            return new EventSummaryDto
            {
                Id = eventModel.Id,
                Name = eventModel.Name,
                EventDate = eventModel.EventDate,
                AttendeeCount = attendeesCount,
                BackgroundImageUri = eventModel.BackgroundImageUri
            };
        }

        public static EventDetailsDto EventToEventDetailsDto(this Event eventModel)
        {
            return new EventDetailsDto
            {
                Id = eventModel.Id,
                Name = eventModel.Name,
                BackgroundImageUri = eventModel.BackgroundImageUri,
                EventDate = eventModel.EventDate,
                Location = eventModel.Location,
                Attendees = eventModel.Attendees.AttendeeToDto(),
                TemplateElements = eventModel.TemplateElements.TemplateElementsToDto(),
                GeneratedInvitationFullPath = eventModel.GeneratedInvitationsZipUri
            };
        }
    }
}