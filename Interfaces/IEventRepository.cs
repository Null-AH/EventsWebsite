using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EventApi.Controllers;
using EventApi.DTO.Event;
using EventApi.DTO.Internal;
using EventApi.DTO.Query;
using EventApi.ExeptionHandling;
using EventApi.Helpers;
using EventApi.Models;

namespace EventApi.Interfaces
{
    public interface IEventRepository
    {
        public Task<Result<Event>> CreateNewEventAsync(NewEventInfo eventInfo);
        public Task<Result<List<EventSummaryDto>>> GetAllUserEventsAsync(AppUser user, EventQueryObject query);
        public Task<Result<EventDetailsDto>> GetEventDetailsByIdAsync(int id, AppUser user);
        public Task<Result<string>> GetEventZipByIdAsync(int id);
        public Task<bool> EventExistsAsync(string userId, CreateEventDto eventInfoToCheck);
        public Task<Result<Event>> DeleteEventByIdAsync(int id);
        public Task<Result<EventSummaryDto>> UpdateEvent(int id, UpdateEventRequestDto updateEventRequestDto, AppUser appUser);

        public Task<EventCheckInResultDto> EventCheckInAsync(int eventId, string userId, EventCheckInRequestDto eventCheckInRequestDto);
        public Task<Result<int?>> GetCurrentCheckedInCountAsync(int eventId, string userId);

        public Task<Result> AddCollaboratorsAsync(List<AddCollaboratorsRequestDto> addCollaboratorsDto, AppUser user, int eventId);
        public Task<Result<List<GetCollaboratorsResponseDto>>> GetCollaboratorsAsync(int eventId, string userId);
        public Task<Result<List<EditCollaboratorRequestDto>>> EditCollaboratorsAsync(List<EditCollaboratorRequestDto> editCollaboratorRequestDto, AppUser user, int eventId);
        public Task<Result<bool>> DeleteCollaboratorsAsync(int eventId, AppUser appUser, List<string> collaboratorsToDeleteId);

        public Task<Result<bool>> LeaveEventAsync(int eventId, AppUser user);

        public Task<Result> CheckPermissionAsync(AppUser appUser, int eventId, Actions action);
        public Task<Result> CheckSubscriptionAsync(AppUser user, Actions action);
        
    }
}