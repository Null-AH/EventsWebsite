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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Hangfire;
using EventApi.ExeptionHandling;
using System.Diagnostics.Eventing.Reader;
using System.Formats.Asn1;
using brevo_csharp.Model;
using SubscriptionTier = EventApi.Models.SubscriptionTier;
using NPOI.SS.Formula.Functions;
using Microsoft.CodeAnalysis.Differencing;

namespace EventApi.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDBContext _context;
        private readonly IFileHandlingService _fileHandle;
        private readonly IWebHostEnvironment _webhost;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<EventRepository> _logger;
        private readonly IEmailSevice _emailService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public EventRepository
        (IEmailSevice emailSevice, ILogger<EventRepository> logger, AppDBContext context
        , IWebHostEnvironment webHost, IFileHandlingService fileHandle, UserManager<AppUser> userManager
        , IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _fileHandle = fileHandle;
            _webhost = webHost;
            _userManager = userManager;
            _logger = logger;
            _emailService = emailSevice;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<Result<Event>> CreateNewEventAsync(NewEventInfo eventInfo)
        {
            var user = eventInfo.CreatingUser;
            var backgroundImageString = string.Empty;

            if (eventInfo.BackgroundImage != null)
            {
                var imageSaveResult = await _fileHandle.SaveImageAsync(eventInfo.BackgroundImage);
                if (!imageSaveResult.IsSuccess)
                {
                    return Result<Event>.Failure(imageSaveResult.Error);
                }
                backgroundImageString = imageSaveResult.Value;                
            }
            else
            {
                var eventNameEncoded = System.Net.WebUtility.UrlEncode(eventInfo.EventDetails.Name);
                backgroundImageString = $"https://placehold.co/600x400/020618/FFFFFF/png?text={eventNameEncoded}";
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

            var fileParseResult = await _fileHandle.ParseAttendeesAsync(eventInfo.AttendeeFile);

            if (!fileParseResult.IsSuccess)
            {
                return Result<Event>.Failure(fileParseResult.Error);
            }
            List<Attendee> attendees = fileParseResult.Value;

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

            return Result<Event>.Success(eventModel);
        }

        public async Task<Result<List<EventSummaryDto>>> GetAllUserEventsAsync(AppUser user, EventQueryObject query)
        {
            var userEvents = _context.EventCollaborators.Include(e => e.Event)
            .ThenInclude(e => e.Attendees)
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

            var userFilteredEventes = await userEvents
                .Skip(skipNumber).Take(query.PageSize)
                .Select(u => u.EventToSummaryDto(u.Event.Attendees.Count()))
                .ToListAsync();

            return Result<List<EventSummaryDto>>.Success(userFilteredEventes);
        }

        public async Task<Result<EventDetailsDto>> GetEventDetailsByIdAsync(int id, AppUser user)
        {
            var eventModel = await _context.EventCollaborators
            .Include(e => e.Event).ThenInclude(e => e.Attendees).Include(e => e.Event.TemplateElements).AsSplitQuery()
            .FirstOrDefaultAsync(e => e.EventId == id && e.UserId == user.Id);

            if (eventModel == null)
            {
                return Result<EventDetailsDto>.Failure(EventErrors.EventIdNotFound);
            }

            return Result<EventDetailsDto>.Success(eventModel.EventToEventDetailsDto());
        }

        public async Task<Result<string>> GetEventZipByIdAsync(int id)
        {
            var eventModel = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);

            if (eventModel == null)
            {
                return Result<string>.Failure(EventErrors.EventIdNotFound);
            }

            return Result<string>.Success(eventModel.GeneratedInvitationsZipUri);
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

        public async Task<Result<Event>> DeleteEventByIdAsync(int id)
        {
            var eventModel = await _context.Events
            .Include(e => e.Attendees).Include(e => e.TemplateElements)
            .FirstOrDefaultAsync(e => e.Id == id);

            if (eventModel == null)
            {
                return Result<Event>.Failure(EventErrors.EventIdNotFound);
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
            return Result<Event>.Success(eventModel);
        }

        public async Task<Result<EventSummaryDto>> UpdateEvent(int id, UpdateEventRequestDto updateEventRequestDto, AppUser appUser)
        {
            var eventModel = await _context.EventCollaborators
            .Include(e => e.Event.Attendees)
            .FirstOrDefaultAsync(e => e.EventId == id && e.UserId == appUser.Id);
            if (eventModel == null)
            {
                return Result<EventSummaryDto>.Failure(EventErrors.EventIdNotFound);
            }

            eventModel.Event.Name = updateEventRequestDto.Name;
            eventModel.Event.EventDate = updateEventRequestDto.EventDate;

            await _context.SaveChangesAsync();

            var editedEventResult = eventModel.EventToSummaryDto(eventModel.Event.Attendees.Count());
            return Result<EventSummaryDto>.Success(editedEventResult);
        }

        public async Task<EventCheckInResultDto> EventCheckInAsync
        (int eventId, string userId, EventCheckInRequestDto eventCheckInRequestDto)
        {
            var attendee = await _context.Attendees
            .FirstOrDefaultAsync(e => e.EventId == eventId &&
                                e.Email == eventCheckInRequestDto.email
                                );


            if (attendee == null) return new EventCheckInResultDto
            {
                Status = "NotFound"
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

        public async Task<Result<int?>> GetCurrentCheckedInCountAsync(int eventId, string userId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.Id == eventId);

            if (!eventExists)
            {
                return Result<int?>.Failure(EventErrors.EventIdNotFound);
            }

            var countResult = await _context.Attendees
                .CountAsync(a => a.EventId == eventId && a.ChechkedIn);

            return Result<int?>.Success(countResult);
        }

        public async Task<Result> AddCollaboratorsAsync
        (List<AddCollaboratorsRequestDto> addCollaboratorsDto
        , AppUser user
        , int eventId)
        {

            var eventModel = await _context.Events.FindAsync(eventId);
            if (eventModel == null)
                return Result.Failure(EventErrors.EventIdNotFound);

            List<string> collaboratorsEmails = addCollaboratorsDto.Select(e => e.Email.ToUpperInvariant()).Distinct().ToList();

            addCollaboratorsDto = addCollaboratorsDto.GroupBy(dto => dto.Email.ToUpperInvariant()).Select(g => g.First()).ToList();
            if (!collaboratorsEmails.Any())
            {
                return Result.Success();
            }

            var existingCollaboratorEmails = await _context.EventCollaborators
            .Where(ec => ec.EventId == eventId && ec.AppUser != null && collaboratorsEmails.Contains(ec.AppUser.Email))
            .Select(ec => ec.AppUser.Email.ToUpperInvariant()).ToListAsync();

            var pendingInvitationEmails = await _context.CollaboratorsInvitations
            .Where(e => collaboratorsEmails.Contains(e.InvitedEmail) && e.EventId == eventId)
            .Select(e => e.InvitedEmail.ToUpperInvariant()).ToListAsync();

            var emailsToProcess = collaboratorsEmails.Except(existingCollaboratorEmails)
            .Except(pendingInvitationEmails).ToList();

            if (!emailsToProcess.Any()) return Result.Success();

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
                        InvitedEmail = dto.Email,
                        Status = Status.Pending,
                        Role = EnumUtils.ParseEnumMember<Role>(dto.Role),
                        EventId = eventId,
                        InvitationToken = Guid.NewGuid().ToString()
                    });

            if (collaboratorsToInvite.Any())
            {
                await _context.CollaboratorsInvitations.AddRangeAsync(collaboratorsToInvite);
            }

            if (collaboratorsToInvite.Any() || collaboratorsToAdd.Any())
            {
                await _context.SaveChangesAsync();
                foreach (var invitation in collaboratorsToInvite)
                {
                    _backgroundJobClient.Enqueue<IEmailSevice>(service => service.SendCollaboratorInvitationEmailAsync(
                        invitation.InvitedEmail,
                        eventModel.Name,
                        invitation.Role,
                        invitation.InvitationToken
                    ));
                }
            }


            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<List<GetCollaboratorsResponseDto>>> GetCollaboratorsAsync(int eventId, string userId)
        {
            var collaborators = await _context.EventCollaborators.Where(e => e.EventId == eventId).ToListAsync();

            var collaboratorsUsersId = collaborators
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


            return Result<List<GetCollaboratorsResponseDto>>.Success(GetCollaboratorsResponseDto);
        }

        public async Task<Result<bool>> DeleteCollaboratorsAsync(int eventId, AppUser appUser, List<string> collaboratorsToDeleteId)
        {
            var collaboratorsToDelete = await _context.EventCollaborators.Include(e => e.Event)
            .Where(e => e.EventId == eventId && collaboratorsToDeleteId.Contains(e.UserId)).ToListAsync();

            if (!collaboratorsToDelete.Any())
            {
                return Result<bool>.Failure(CollaboratorErrors.CollaboratorsNotFound);
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
                        return Result<bool>.Failure(CollaboratorErrors.DeleteAllOwners);
                    }

                    bool isCurrentUserTheCreator = (creatorUserId == appUser.Id);
                    if (isCreatorPresent)
                    {
                        if (!isCurrentUserTheCreator)
                        {
                            return Result<bool>.Failure(CollaboratorErrors.NonCreatorOwnerDelete);
                        }
                    }
                }
            }

            _context.EventCollaborators.RemoveRange(collaboratorsToDelete);

            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        public async Task<Result<List<EditCollaboratorRequestDto>>> EditCollaboratorsAsync
        (List<EditCollaboratorRequestDto> editCollaboratorsRequestDto, AppUser user, int eventId)
        {
            var collaboratorsToEditIds = editCollaboratorsRequestDto.Select(e => e.UserId);

            var collaboratorsToEdit = await _context.EventCollaborators.Include(e => e.Event)
            .Where(e => e.EventId == eventId && collaboratorsToEditIds.Contains(e.UserId)).ToListAsync();

            var collaboratorsToEditRoles = collaboratorsToEdit.Select(e => e.Role).ToList();

            var newRoles = editCollaboratorsRequestDto.Select(e => EnumUtils.ParseEnumMember<Role>(e.Role)).ToList();

            if (!collaboratorsToEdit.Any())
            {
                return Result<List<EditCollaboratorRequestDto>>.Failure(CollaboratorErrors.CollaboratorsNotFound);
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
                    return Result<List<EditCollaboratorRequestDto>>.Failure(CollaboratorErrors.CollaboratorLastOwnerEditDelete);
                }

                if (isCreatorPresent)
                {
                    if (!isSelfDemotion && !isCurrentUserTheCreator && collaborator.Role == Role.Owner)
                    {
                        return Result<List<EditCollaboratorRequestDto>>.Failure(CollaboratorErrors.CollaboratorCreatorEditDelete);
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

            return Result<List<EditCollaboratorRequestDto>>.Success(editedCollaborators);
        }

        public async Task<Result<bool>> LeaveEventAsync(int eventId, AppUser user)
        {
            var collaboratorToLeave = await _context.EventCollaborators
            .FirstOrDefaultAsync(e => e.EventId == eventId && e.UserId == user.Id);

            if (collaboratorToLeave == null)
            {
                return Result<bool>.Failure(CollaboratorErrors.CollaboratorsNotFound);
            }

            var ownerCount = await _context.EventCollaborators.CountAsync(ec => ec.EventId == eventId && ec.Role == Role.Owner);

            if (ownerCount <= 1 && collaboratorToLeave.Role == Role.Owner)
            {
                return Result<bool>.Failure(CollaboratorErrors.CollaboratorLastOwnerEditDelete);
            }

            _context.EventCollaborators.Remove(collaboratorToLeave);

            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }



        public async Task<Result> CheckPermissionAsync(AppUser appUser, int eventId, Actions action)
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
                return Result.Failure(CollaboratorErrors.CollaboratorsNotFound);
            }
            _logger.LogInformation("PERMISSION CHECK SUCCESS: Found role '{Role}' for user.", usersRole.Role);

            switch (usersRole.Role)
            {
                case Role.Owner:
                    return Result.Success();

                case Role.Editor:
                    if (editorNotPermitedActions.Contains(action))
                    {
                        return Result.Failure(CollaboratorErrors.EditorNotPermitedAction);
                    }
                    break;

                case Role.CheckInStaff:
                    if (checkInStaffNotPermitedActions.Contains(action))
                    {
                        return Result.Failure(CollaboratorErrors.CheckInStaffNotPermitedAction);
                    }
                    break;
                default:
                    return Result.Failure(CollaboratorErrors.GeneralNotPermitedAction);
            }
            return Result.Success();
        }

        public async Task<Result> CheckSubscriptionAsync(AppUser user, Actions action)
        {
            List<Actions> freeUserNotAllowedActions = new List<Actions>()
            {
                Actions.CreatePro,
                Actions.EventDownloadZip,
            };
            List<Actions> proUserAllowedActions = new List<Actions>()
            {
            };

            //Event limit check
            var userEventsCount = await _context.EventCollaborators.CountAsync(e => e.UserId == user.Id);

            if (freeUserNotAllowedActions.Contains(action))
            {
                return Result.Failure(UserErrors.SubscriptionLimit);
            }
            if (proUserAllowedActions.Contains(action))
            {
                return Result.Failure(UserErrors.SubscriptionLimit);
            }

            switch (user.Tier)
            {
                case SubscriptionTier.Free:
                    {
                        if (userEventsCount >= 5)
                        {
                            return Result.Failure(EventErrors.LimitReached);
                        }
                        return Result.Success();
                    }
                case SubscriptionTier.Pro:
                    {
                        if (userEventsCount >= 10)
                        {
                            return Result.Failure(EventErrors.LimitReached);
                        }
                        return Result.Success();
                    }
                default: return Result.Success();
            }            
        }


    }
    

}