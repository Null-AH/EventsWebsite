using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.DTO.Event;
using EventApi.Migrations;
using EventApi.Models;
using EventApi.Services;
using Humanizer.DateTimeHumanizeStrategy;
using NPOI.SS.Formula.Functions;
using EventApi.Helpers;

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

        public static EventSummaryDto EventToSummaryDto(this EventCollaborators eventCollabModel, int attendeesCount)
        {
            return new EventSummaryDto
            {
                Id = eventCollabModel.Event.Id,
                Name = eventCollabModel.Event.Name,
                EventDate = eventCollabModel.Event.EventDate,
                AttendeeCount = attendeesCount,
                BackgroundImageUri = eventCollabModel.Event.BackgroundImageUri,
                Role = eventCollabModel.Role.GetEnumMemberValue()
            };
        }
        public static EventDetailsDto EventToEventDetailsDto(this EventCollaborators eventModel)
        {
            return new EventDetailsDto
            {
                Id = eventModel.Event.Id,
                Name = eventModel.Event.Name,
                BackgroundImageUri = eventModel.Event.BackgroundImageUri,
                EventDate = eventModel.Event.EventDate,
                Location = eventModel.Event.Location,
                Attendees = eventModel.Event.Attendees.AttendeeToDto(),
                TemplateElements = eventModel.Event.TemplateElements.TemplateElementsToDto(),
                GeneratedInvitationFullPath = eventModel.Event.GeneratedInvitationsZipUri,
                Role = eventModel.Role.GetEnumMemberValue()
            };
        }

    }
}