using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using EventApi.Controllers;
using EventApi.Data;
using EventApi.Interfaces;
using EventApi.Mappers;
using EventApi.Models;
using Microsoft.EntityFrameworkCore;
using EventApi.DTO.Internal;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using EventApi.DTO.Event;
using EventApi.DTO.Query;
using NPOI.HSSF.Record;
using System.Linq.Expressions;
using MathNet.Numerics.Financial;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using EventApi.Services;
using EventApi.Helpers;
using MathNet.Numerics;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Http.HttpResults;
using EventApi.Migrations;
using System.Xml;

namespace EventApi.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDBContext _context;
        private readonly IFileHandlingService _fileHandle;
        private readonly IWebHostEnvironment _webhost;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<EventRepository> _logger;

        public EventRepository(ILogger<EventRepository> logger, AppDBContext context, IWebHostEnvironment webHost, IFileHandlingService fileHandle, UserManager<AppUser> userManager)
        {
            _context = context;
            _fileHandle = fileHandle;
            _webhost = webHost;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Event> CreateNewEventAsync(NewEventInfo eventInfo)
        {
            var user = eventInfo.CreatingUser;
            var backgroundImageString = string.Empty;

            if (eventInfo.BackgroundImage != null)
            {

                backgroundImageString = await _fileHandle.SaveImageAsync(eventInfo.BackgroundImage);
            }
            else
            {
                var eventNameEncoded = System.Net.WebUtility.UrlEncode(eventInfo.EventDetails.Name);
                backgroundImageString = $"https://placehold.co/600x400/2B2543/FFFFFF/png?text={eventNameEncoded}";
            }

            var eventModel = eventInfo.EventDetails.ToEventFromEventRequestDto(eventInfo.CreatingUser, backgroundImageString);

            await _context.Events.AddAsync(eventModel);
            await _context.SaveChangesAsync();

            if (eventInfo.TemplateElements != null)
            {

                var templateModel = eventInfo.TemplateElements.ToTemplateElementsFromTemplateDto(eventModel.Id);

                await _context.TemplateElements.AddRangeAsync(templateModel);
                await _context.SaveChangesAsync();
            }

            List<Attendee> attendees = await _fileHandle.ParseAttendeesAsync(eventInfo.AttendeeFile);

            foreach (var attendee in attendees)
            {
                attendee.EventId = eventModel.Id;
            }

            await _context.Attendees.AddRangeAsync(attendees);

            var addToCollaborators = new EventCollaborators
            {
                UserId = user.Id,
                EventId = eventModel.Id,
                Role = Role.Owner
            };

            await _context.EventCollaborators.AddAsync(addToCollaborators);

            await _context.SaveChangesAsync();
            return eventModel;
        }

        public async Task<List<EventSummaryDto>> GetAllUserEventsAsync(AppUser user, EventQueryObject query)
        {
            var userEvents = _context.EventCollaborators.Include(e => e.Event).ThenInclude(e => e.Attendees)
            .Where(e => e.UserId == user.Id);

            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                userEvents = userEvents.Where(e => e.Event.Name.Contains(query.Name));
            }

            var sortExpresions = new Dictionary<string, Expression<Func<EventCollaborators, object>>>
            {
                {"name",e => e.Event.Name},
                {"eventdate",e => e.Event.EventDate}
            };

            var selectedSortExpression = sortExpresions.GetValueOrDefault(query.OrderBy.ToLower(), e => e.Event.EventDate);

            if (!string.IsNullOrWhiteSpace(query.OrderBy))
            {
                userEvents = query.IsDescending ? userEvents.OrderByDescending(selectedSortExpression) 
                : userEvents.OrderBy(selectedSortExpression);
            }

            var skipNumber = (query.PageNumber - 1) * query.PageSize;

            return await userEvents
                .Skip(skipNumber).Take(query.PageSize)
                .Select(u => u.EventToSummaryDto(u.Event.Attendees.Count()))
                .ToListAsync();
        }

        public async Task<EventDetailsDto> GetEventDetailsByIdAsync(int id,AppUser user)
        {
            var eventModel = await _context.EventCollaborators
            .Include(e => e.Event).ThenInclude(e => e.Attendees).Include(e => e.Event.TemplateElements).AsSplitQuery()
            .FirstOrDefaultAsync(e => e.EventId == id && e.UserId == user.Id);

            if (eventModel == null)
            {
                return null;
            }

            return eventModel.EventToEventDetailsDto();
        }

        public async Task<Event> GetEventByIdAsync(int id)
        {
            var eventModel = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);

            if (eventModel == null)
            {
                return null;
            }

            return eventModel;
        }

        public async Task<bool> EventExistsAsync(string userId, CreateEventDto eventInfoToCheck)
        {
            var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);

            return await _context.Events.AnyAsync(e =>
                e.Name == eventInfoToCheck.Name &&
                e.AppUserId == userId &&
                e.EventDate == eventInfoToCheck.EventDate &&
                e.CreatedAt >= fiveMinutesAgo
                );
        }

        public async Task<Event> DeleteEventByIdAsync(int id)
        {
            var eventModel = await _context.Events.Include(e => e.Attendees).Include(e => e.TemplateElements).FirstOrDefaultAsync(e => e.Id == id);

            if (eventModel == null)
            {
                return null;
            }

            var attendees = eventModel.Attendees.Select(i => i.Id);
            // var invitations = await _context.Invitations.Where(i => attendees.Contains(i.AttendeeId)).ToListAsync();

            // if (invitations.Any())
            // {
            //     _context.Invitations.RemoveRange(invitations);
            // }
            if (eventModel.Attendees.Any())
            {
                _context.Attendees.RemoveRange(eventModel.Attendees);
            }
            if (eventModel.TemplateElements.Any())
            {
                _context.TemplateElements.RemoveRange(eventModel.TemplateElements);
            }
            if (!string.IsNullOrEmpty(eventModel.BackgroundImageUri))
            {
                var imagePath = Path.Combine(_webhost.WebRootPath, eventModel.BackgroundImageUri.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            if (!string.IsNullOrEmpty(eventModel.GeneratedInvitationsZipUri))
            {
                var zipPath = Path.Combine(_webhost.WebRootPath, eventModel.GeneratedInvitationsZipUri.TrimStart('/'));
                if (System.IO.File.Exists(zipPath))
                {
                    System.IO.File.Delete(zipPath);
                }
            }

            var removeCollaborators = await _context.EventCollaborators.Where(e => e.EventId == eventModel.Id).ToListAsync();

            if (removeCollaborators.Any())
            {                
                _context.EventCollaborators.RemoveRange(removeCollaborators);
            }
            _context.Events.Remove(eventModel);

            await _context.SaveChangesAsync();
            return eventModel;
        }

        public async Task<EventSummaryDto> UpdateEvent(int id, UpdateEventRequestDto updateEventRequestDto,AppUser appUser)
        {
            var eventModel = await _context.EventCollaborators.Include(e => e.Event.Attendees).FirstOrDefaultAsync(e => e.EventId == id && e.UserId == appUser.Id);
            if (eventModel == null)
            {
                return null;
            }

            eventModel.Event.Name = updateEventRequestDto.Name;
            eventModel.Event.EventDate = updateEventRequestDto.EventDate;

            await _context.SaveChangesAsync();

            return eventModel.EventToSummaryDto(eventModel.Event.Attendees.Count());
        }

        public async Task<EventCheckInResultDto> EventCheckInAsync(int eventId, string userId, EventCheckInRequestDto eventCheckInRequestDto)
        {
            var attendee = await _context.Attendees
            .FirstOrDefaultAsync(e => e.EventId == eventId &&
                                e.Email == eventCheckInRequestDto.email
                                );


            if (attendee == null) return new EventCheckInResultDto
            {
                Status = "NotFound",
            };

            var currentCount = await _context.Attendees.CountAsync(a => a.EventId == eventId && a.ChechkedIn);

            if (attendee.ChechkedIn)
            {
                return new EventCheckInResultDto
                {
                    Status = "AlreadyCheckedIn",
                    AttendeeName = attendee.Name,
                    CheckedInCount = currentCount
                };
            }

            attendee.ChechkedIn = true;
            attendee.CheckedInTimestamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();


            return new EventCheckInResultDto
            {
                Status = "Success",
                AttendeeName = attendee.Name,
                CheckedInCount = currentCount + 1
            };
        }

        public async Task<int?> GetCurrentCheckedInCountAsync(int eventId, string userId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.Id == eventId);

            if (!eventExists)
            {
                return null;
            }

            return await _context.Attendees
                .CountAsync(a => a.EventId == eventId && a.ChechkedIn);
        }

        public async Task AddCollaboratorsAsync
        (List<AddCollaboratorsRequestDto> addCollaboratorsDto
        , AppUser user
        , int eventId)
        {

            List<string> collaboratorsEmails = addCollaboratorsDto.Select(e => e.Email.ToUpperInvariant()).Distinct().ToList();

            if (!collaboratorsEmails.Any())
            {
                return;
            }
            var existingCollaboratorEmails = await _context.EventCollaborators
            .Where(ec => ec.EventId == eventId && ec.AppUser != null && collaboratorsEmails.Contains(ec.AppUser.Email))
            .Select(ec => ec.AppUser.Email.ToUpperInvariant()).ToListAsync();

            var pendingInvitationEmails = await _context.CollaboratorsInvitations
            .Where(e => collaboratorsEmails.Contains(e.InvitedEmail) && e.EventId == eventId)
            .Select(e => e.InvitedEmail.ToUpperInvariant()).ToListAsync();

            var emailsToProcess = collaboratorsEmails.Except(existingCollaboratorEmails)
            .Except(pendingInvitationEmails).ToList();

            if (!emailsToProcess.Any()) return;

            var existingUsers = await _userManager.Users
            .Where(e => e.Email != null && emailsToProcess.Contains(e.Email)).ToListAsync();

            var existingUsersEmails = existingUsers.Select(e => e.Email.ToUpperInvariant()).ToList();

            var collaboratorsToAdd = addCollaboratorsDto
                .Join(existingUsers, dto => dto.Email.ToUpperInvariant(), user => user.Email.ToUpperInvariant()
                , (dto, user) => new EventCollaborators
                {
                    UserId = user.Id,
                    Role = EnumUtils.ParseEnumMember<Role>(dto.Role),
                    EventId = eventId
                });
            await _context.EventCollaborators.AddRangeAsync(collaboratorsToAdd);

            var nonExistingUsersEmails = collaboratorsEmails.Where(email => !existingUsersEmails.Contains(email));

            var collaboratorsToInvite = addCollaboratorsDto
            .Join(nonExistingUsersEmails, dto => dto.Email.ToUpperInvariant(), user => user.ToUpperInvariant()
                    , (dto, user) => new CollaboratorsInvitation
                    {
                        InvitedEmail = user,
                        Status = Status.Pending,
                        Role = EnumUtils.ParseEnumMember<Role>(dto.Role),
                        EventId = eventId,
                        InvitationToken = Guid.NewGuid().ToString()
                    });

            if (collaboratorsToInvite.Any())
            {

                await _context.CollaboratorsInvitations.AddRangeAsync(collaboratorsToInvite);
            }

            await _context.SaveChangesAsync();
            return;
        }

        public async Task<List<GetCollaboratorsResponseDto>> GetCollaboratorsAsync(int eventId, string userId)
        {
            var collaborators = await _context.EventCollaborators.Where(e => e.EventId == eventId).ToListAsync();

            var collaboratorsUsersId =  collaborators
            .Where(e => e.EventId == eventId).Select(e => e.UserId).ToList();
            var appUserInfo = await _userManager.Users.Where(u => collaboratorsUsersId.Contains(u.Id)).ToListAsync();

            var GetCollaboratorsResponseDto = appUserInfo
            .Join(collaborators, user => user.Id, coll => coll.UserId, (user, coll) =>
            new GetCollaboratorsResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = coll.Role.GetEnumMemberValue(),
                DisplayName = user.DisplayName ?? user.Email,
                PictureUrl = user.PictureUrl
            }).ToList();
            

            return GetCollaboratorsResponseDto;
        }

        public async Task<bool> DeleteCollaboratorsAsync(int eventId, AppUser appUser, List<string> collaboratorsToDeleteId)
        {
            var collaboratorsToDelete = await _context.EventCollaborators.Include(e => e.Event)
            .Where(e => e.EventId == eventId && collaboratorsToDeleteId.Contains(e.UserId)).ToListAsync();
           
            if (!collaboratorsToDelete.Any())
            {
                return false;
            }

            var collaboratorsRoles = collaboratorsToDelete.Select(e => e.Role).ToList();

            var ownerCount = await _context.EventCollaborators.CountAsync(ec => ec.EventId == eventId && ec.Role == Role.Owner);
            var creatorUserId = collaboratorsToDelete.First().Event.AppUserId;

            bool isCreatorPresent = await _context.EventCollaborators.AnyAsync(ec => ec.EventId == eventId && ec.Event.AppUserId == creatorUserId);

            foreach (var collaborator in collaboratorsToDelete)
            {
                if (collaborator.Role == Role.Owner)
                {
                    if (ownerCount <= collaboratorsToDelete.Count(c => c.Role == Role.Owner))
                    {
                        throw new InvalidOperationException("You cannot remove all owners of an event. Please leave at least one.");
                    }

                    bool isCurrentUserTheCreator = (creatorUserId == appUser.Id);
                    if (isCreatorPresent)
                    {
                        if (!isCurrentUserTheCreator)
                        {
                            throw new UnauthorizedAccessException("Only the event creator can remove another owner while they are still a collaborator.");
                        }
                    }
                }
            }

            _context.EventCollaborators.RemoveRange(collaboratorsToDelete);

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<EditCollaboratorRequestDto>> EditCollaboratorsAsync(List<EditCollaboratorRequestDto> editCollaboratorsRequestDto, AppUser user, int eventId)
        {
            var collaboratorsToEditIds = editCollaboratorsRequestDto.Select(e => e.UserId);

            var collaboratorsToEdit = await _context.EventCollaborators.Include(e => e.Event)
            .Where(e => e.EventId == eventId && collaboratorsToEditIds.Contains(e.UserId)).ToListAsync();

            var collaboratorsToEditRoles = collaboratorsToEdit.Select(e => e.Role).ToList();

            var newRoles = editCollaboratorsRequestDto.Select(e => EnumUtils.ParseEnumMember<Role>(e.Role)).ToList();

            if (!collaboratorsToEdit.Any())
            {
                var foundIds = collaboratorsToEdit.Select(c => c.UserId);
                var notFoundIds = collaboratorsToEditIds.Except(foundIds);
                // This gives a super helpful error message like "Collaborators not found: [guid1, guid2]"
                throw new KeyNotFoundException($"Collaborators not found for the following IDs: {string.Join(", ", notFoundIds)}");
            }
            
            var ownerCount = await _context.EventCollaborators.CountAsync(ec => ec.EventId == eventId && ec.Role == Role.Owner);

            var creatorUserId = collaboratorsToEdit.First().Event.AppUserId;
            bool isCreatorPresent = await _context.EventCollaborators.AnyAsync(ec => ec.EventId == eventId && ec.Event.AppUserId == creatorUserId);

            
            foreach (var collaborator in collaboratorsToEdit)
            {
                var dto = editCollaboratorsRequestDto.First(d => d.UserId == collaborator.UserId);
                var newRole = EnumUtils.ParseEnumMember<Role>(dto.Role);

                bool isSelfDemotion = (collaborator.UserId == user.Id);
                bool isCurrentUserTheCreator = (creatorUserId == user.Id);

                if (ownerCount <= 1 && newRole != Role.Owner && collaborator.Role == Role.Owner)
                {
                    throw new InvalidOperationException("Cannot remove or downgrade the last owner of an event. Please assign a new owner first.");
                }

                if (isCreatorPresent)
                {
                    if (!isSelfDemotion && !isCurrentUserTheCreator && collaborator.Role == Role.Owner)
                    {
                        throw new UnauthorizedAccessException("Only the event creator can demote another owner.");
                    }
                }

                collaborator.Role = newRole;
            }

            await _context.SaveChangesAsync();

            var editedCollaborators = collaboratorsToEdit.Select(c => new EditCollaboratorRequestDto
            {
                UserId = c.UserId,
                Role = c.Role.GetEnumMemberValue(),
                Email = editCollaboratorsRequestDto.Find(e => e.UserId == c.UserId).Email
            }).ToList();
            
            return editedCollaborators;
        }

        public async Task<bool> LeaveEventAsync(int eventId, AppUser user)
        {
            var collaboratorToLeave = await _context.EventCollaborators
            .FirstOrDefaultAsync(e => e.EventId == eventId && e.UserId == user.Id);
           
            if (collaboratorToLeave == null)
            {
                return false;
            }

            var ownerCount = await _context.EventCollaborators.CountAsync(ec => ec.EventId == eventId && ec.Role == Role.Owner);            

            if (ownerCount <= 1 && collaboratorToLeave.Role == Role.Owner)
            {
                throw new InvalidOperationException("You are the Only owner for this event. Please assign a new owner first or just delete it.");
            }

            _context.EventCollaborators.Remove(collaboratorToLeave);

            await _context.SaveChangesAsync();
            return true;
        }



        public async Task CheckPermissionAsync(AppUser appUser, int eventId, Actions action)
        {
            _logger.LogInformation("--- PERMISSION CHECK START ---");
            _logger.LogInformation("Checking permission for AppUser ID: {UserId}, Event ID: {EventId}, Action: {Action}", appUser.Id, eventId, action);

            List<Actions> editorNotPermitedActions = new List<Actions>()
            {
                Actions.AddCollaborators,
                Actions.DeleteCollaborators,
                Actions.EventDelete,
                Actions.EditCollaborators
            };
            List<Actions> checkInStaffNotPermitedActions = new List<Actions>()
            {
                Actions.EventGetById,
                Actions.EventDownloadZip,
                Actions.EventEdit,
                Actions.EventDelete,
                Actions.GetCollaborators,
                Actions.AddCollaborators,
                Actions.EditCollaborators,
                Actions.DeleteCollaborators
            };

            var usersRole = await _context.EventCollaborators
            .FirstOrDefaultAsync(e => e.EventId == eventId && e.UserId == appUser.Id);

            if (usersRole == null)
            {
                _logger.LogWarning("PERMISSION CHECK FAILED: No EventCollaborators entry found for AppUser ID: {UserId} and Event ID: {EventId}. Throwing UnauthorizedAccessException.", appUser.Id, eventId);
                throw new UnauthorizedAccessException($"You are not a collaborator on this event and have no permissions");
            }
            _logger.LogInformation("PERMISSION CHECK SUCCESS: Found role '{Role}' for user.", usersRole.Role);

            switch (usersRole.Role)
            {
                case Role.Owner:
                    return;

                case Role.Editor:
                    if (editorNotPermitedActions.Contains(action))
                    {
                        throw new UnauthorizedAccessException($"As an editor You do not have permission to perform the action: {action}");
                    }
                    break;

                case Role.CheckInStaff:
                    if (checkInStaffNotPermitedActions.Contains(action))
                    {
                        throw new UnauthorizedAccessException($"As a Check-In staff You do not have permission to perform the action: {action}");
                    }
                    break;
                default:
                    throw new UnauthorizedAccessException($"Your role does not grant You permission to perform the action: {action}");
            }
        }
    }
}