using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EventApi.Controllers;
using EventApi.DTO.Event;
using EventApi.DTO.Internal;
using EventApi.DTO.Query;
using EventApi.Models;

namespace EventApi.Interfaces
{
    public interface IEventRepository
    {
        public Task<Event> CreateNewEventAsync(NewEventInfo eventInfo);
        public Task<List<EventSummaryDto>> GetAllUserEventsAsync(AppUser user, EventQueryObject query);
        public Task<EventDetailsDto> GetEventDetailsByIdAsync(int id);
        public Task<Event> GetEventByIdAsync(int id);
        public Task<bool> EventExistsAsync(string userId, CreateEventDto eventInfoToCheck);
        public Task<Event> DeleteEventByIdAsync(int id);
        public Task<EventSummaryDto> UpdateEvent(int id, UpdateEventRequestDto updateEventRequestDto);
        public Task<EventCheckInResultDto> EventCheckInAsync(int eventId, string userId, EventCheckInRequestDto eventCheckInRequestDto);
        public Task<int?> GetCurrentCheckedInCountAsync(int eventId, string userId);
        public Task AddCollaboratorsAsync(List<AddCollaboratorsRequestDto> addCollaboratorsDto, AppUser user,int eventId);
    }
}