using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;
using EventApi.DTO.AttendeeDTO;
using EventApi.DTO.Event;
using EventApi.DTO.Internal;
using EventApi.DTO.Query;
using EventApi.DTO.Template;
using EventApi.ExeptionHandling;
using EventApi.Filters;
using EventApi.Helpers;
using EventApi.Interfaces;
using EventApi.Models;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using NPOI.SS.Formula.Functions;

namespace EventApi.Controllers
{
    [Route("api/event")]
    [ApiController]
    public class EventController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEventRepository _eventRepo;
        private readonly IImageGenerationService _imageGen;
        private readonly IBackgroundJobClient _backgroundJob;
        private readonly ILogger<EventController> _logger;
        public EventController(UserManager<AppUser> userManager, ILogger<EventController> logger, IEventRepository eventrepo, IImageGenerationService imageGen, IBackgroundJobClient backgroundJob)
        {
            _eventRepo = eventrepo;
            _userManager = userManager;
            _imageGen = imageGen;
            _backgroundJob = backgroundJob;
            _logger = logger;
        }

        [HttpPost("create-free")]
        [Authorize]
        [LoadUser]
        [SubscriptionCheck(Actions.CreateFree)]
        public async Task<IActionResult> CreateFree([FromForm] CreateEventRequestDto createEventRequestDto)
        {
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var createEventDto = JsonSerializer.Deserialize<CreateEventDto>(createEventRequestDto.EventInfo);
            var user = HttpContext.Items["AppUser"] as AppUser;

            // if (await _eventRepo.EventExistsAsync(user.Id, createEventDto))
            // {
            //     return Conflict("An event with this name and date has already been created.");
            // }

            var newEventInfo = new NewEventInfo
            {
                EventDetails = createEventDto,
                TemplateElements = null,
                AttendeeFile = createEventRequestDto.AttendeeFile,
                BackgroundImage = null,
                CreatingUser = user
            };

            var createdEventResult = await _eventRepo.CreateNewEventAsync(newEventInfo);

            if (createdEventResult.IsSuccess)
            {
                return CreatedAtAction
                (nameof(GetById), new { id = createdEventResult.Value.Id },
                 createdEventResult.Value);
            }

            return HandleResult(createdEventResult);

        }


        [HttpPost("create")]
        [Authorize]
        [LoadUser]
        [SubscriptionCheck(Actions.CreatePro)]
        public async Task<IActionResult> Create([FromForm] CreateEventRequestDto eventRequestDto)
        {

            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var createEventDto = JsonSerializer
            .Deserialize<CreateEventDto>(eventRequestDto.EventInfo, serializerOptions);

            var templateElementsDto = JsonSerializer
            .Deserialize<List<TemplateElementsDto>>(eventRequestDto.TemplateInfo, serializerOptions);

            var user = HttpContext.Items["AppUser"] as AppUser;


            // if (await _eventRepo.EventExistsAsync(user.Id, createEventDto))
            // {
            //     return Conflict("An event with this name and date has already been created.");
            // }

            var newEventInfo = new NewEventInfo
            {
                EventDetails = createEventDto,
                TemplateElements = templateElementsDto,
                AttendeeFile = eventRequestDto.AttendeeFile,
                BackgroundImage = eventRequestDto.BackgroundImage,
                CreatingUser = user
            };


            var createdEventResult = await _eventRepo.CreateNewEventAsync(newEventInfo);

            if (createdEventResult.IsSuccess)
            {
                _backgroundJob.Enqueue<IImageGenerationService>(service => service
                .GenerateInvitationsForEventAsync(createdEventResult.Value.Id));

                return AcceptedAtAction(nameof(GetById), new { id = createdEventResult.Value.Id }, createdEventResult.Value);
            }
            return HandleResult(createdEventResult);

            //var zipFilePath = await _imageGen.GenerateInvitationsForEventAsync(createdEvent.Id);
            //var fileBytes = await System.IO.File.ReadAllBytesAsync(zipFilePath);
            //System.IO.File.Delete(zipFilePath);
            //return File(fileBytes, "application/zip", "invitations.zip",enableRangeProcessing : true);

        }


        [HttpGet("all")]
        [Authorize]
        [LoadUser]
        [SubscriptionCheck(Actions.EventGetAll)]
        //[CheckEventPermission(Actions.EventGetAll)]

        [ProducesResponseType(typeof(IEnumerable<EventSummaryDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllUserEvents([FromQuery] EventQueryObject query)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;
            var userEventsResult = await _eventRepo.GetAllUserEventsAsync(user, query);

            return HandleResult(userEventsResult);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        [LoadUser]
        [SubscriptionCheck(Actions.EventGetById)]
        [CheckEventPermission(Actions.EventGetById)]

        [ProducesResponseType(500)]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]

        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;

            var eventDetails = await _eventRepo.GetEventDetailsByIdAsync(id, user);

            return HandleResult(eventDetails);
        }

        [HttpGet("{id:int}/download")]
        [LoadUser]
        [SubscriptionCheck(Actions.EventDownloadZip)]
        [CheckEventPermission(Actions.EventDownloadZip)]

        [ProducesResponseType(500)]
        [ProducesResponseType(200)]
        [ProducesResponseType(202)]
        [ProducesResponseType(401)]

        public async Task<IActionResult> GetDownloadById([FromRoute] int id)
        {
            var eventZipResult = await _eventRepo.GetEventZipByIdAsync(id);

            if (!eventZipResult.IsSuccess)
            {
                return HandleResult(eventZipResult);
            }
            else
            {
                if (string.IsNullOrEmpty(eventZipResult.Value))
                {
                    return Accepted("Processing... Please Check later");
                }
                else
                {
                    return Ok(eventZipResult.Value);
                }
            }
        }

        [HttpPut("edit")]
        [Authorize]
        [LoadUser]
        [CheckEventPermission(Actions.EventEdit)]

        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        [ProducesResponseType(200)]

        public async Task<IActionResult> UpdateEvent([FromBody] UpdateEventRequestDto updateEventRequestDto)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;

            if (!ModelState.IsValid) return BadRequest(ModelState);
            var editedEvent = await _eventRepo.UpdateEvent(updateEventRequestDto.Id, updateEventRequestDto, user);

            return HandleResult(editedEvent);
        }

        [HttpDelete("delete")]
        [Authorize]
        [LoadUser]
        [CheckEventPermission(Actions.EventDelete)]

        [ProducesResponseType(404)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]

        public async Task<IActionResult> DeleteEvent([FromQuery] int id)
        {

            var deletedEventResult = await _eventRepo.DeleteEventByIdAsync(id);
            if (deletedEventResult.IsSuccess)
            {
                return NoContent();
            }

            return HandleResult(deletedEventResult);
        }


        [HttpPost("{id:int}/check-in")]
        [Authorize]
        [LoadUser]
        [SubscriptionCheck(Actions.CheckIn)]
        [CheckEventPermission(Actions.CheckIn)]

        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> QrCheckIn([FromRoute] int id, [FromBody] EventCheckInRequestDto checkInRequestDto)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;
            if (checkInRequestDto == null)
                return BadRequest("Invalid Data!");

            var checkInResult = await _eventRepo.EventCheckInAsync(id, user.Id, checkInRequestDto);

            switch (checkInResult.Status)
            {
                case "Success":
                    return Ok(checkInResult);
                case "AlreadyCheckedIn":
                    return Conflict(checkInResult);
                case "NotFound":
                    return NotFound(checkInResult);
                default:
                    return StatusCode(500, "An unknown error occured during check-in");
            }
        }

        [HttpGet("{id:int}/count")]
        [Authorize]
        [LoadUser]
        [SubscriptionCheck(Actions.Count)]
        [CheckEventPermission(Actions.Count)]
        public async Task<IActionResult> GetCount(int id)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;
            var countResult = await _eventRepo.GetCurrentCheckedInCountAsync(id, user.Id);

            if (!countResult.IsSuccess)
            {
                return HandleResult(countResult);
            }

            return Ok(new { CheckedInCount = countResult.Value });
        }

        [HttpPost("{id:int}/addteam")]
        [Authorize]
        [LoadUser]
        [SubscriptionCheck(Actions.AddCollaborators)]
        [CheckEventPermission(Actions.AddCollaborators)]

        public async Task<IActionResult> AddCollaborators([FromBody] List<AddCollaboratorsRequestDto> addCollaboratorsDto, int id)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;

            var addResult = await _eventRepo.AddCollaboratorsAsync(addCollaboratorsDto, user, id);

            return HandleResult(addResult);
        }

        [HttpPut("{id:int}/editteam")]
        [Authorize]
        [LoadUser]
        [SubscriptionCheck(Actions.EditCollaborators)]
        [CheckEventPermission(Actions.EditCollaborators)]

        public async Task<IActionResult> EditCollaborators([FromBody] List<EditCollaboratorRequestDto> editCollaboratorsDto, int id)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;
            var editedCollaborator = await _eventRepo.EditCollaboratorsAsync
            (editCollaboratorsDto, user, id);

            return HandleResult(editedCollaborator);
        }

        [HttpGet("{id:int}/getteam")]
        [Authorize]
        [LoadUser]
        [SubscriptionCheck(Actions.GetCollaborators)]
        [CheckEventPermission(Actions.GetCollaborators)]
        public async Task<IActionResult> GetCollaborators(int id)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;
            var collaboratorsResponseDto = await _eventRepo.GetCollaboratorsAsync(id, user.Id);
            return HandleResult(collaboratorsResponseDto);
        }

        [HttpPost("{id:int}/deleteteam")]
        [Authorize]
        [LoadUser]
        [SubscriptionCheck(Actions.DeleteCollaborators)]
        [CheckEventPermission(Actions.DeleteCollaborators)]
        public async Task<IActionResult> DeleteCollaborators([FromRoute] int id, [FromBody] List<string> userToDeleteId)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;
            var deleteResult = await _eventRepo.DeleteCollaboratorsAsync(id, user, userToDeleteId);

            if (!deleteResult.IsSuccess)
            {
                return HandleResult(deleteResult);
            }

            return NoContent();
        }

        [HttpPost("{id:int}/leave")]
        [Authorize]
        [LoadUser]        
        [CheckEventPermission(Actions.Leave)]
        public async Task<IActionResult> LeaveEvent([FromRoute] int id)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;
            var leaveResult = await _eventRepo.LeaveEventAsync(id, user);

            if (!leaveResult.IsSuccess)
            {
                return HandleResult(leaveResult);
            }

            return NoContent();
        }

        [HttpPost("{id:int}/invtation-send")]
        [Authorize]
        [LoadUser]
        public async Task<IActionResult> SendAttendeeInvitations([FromRoute] int id)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;
            var sendResult = await _eventRepo.SendAttendeesInvitationsAsync(id, user);

            return HandleResult(sendResult);
        }

        [HttpPost("{id:int}/add-attendee")]
        [Authorize]
        [LoadUser]
        //[SubscriptionCheck(Actions.AddAttendees)]
        [CheckEventPermission(Actions.AddAttendees)]
        public async Task<IActionResult> AddAttendees([FromBody] List<AddAttendeesRequestDto> addedAttendees, int id)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;

            var addResult = await _eventRepo.AddAttendeesAsync(addedAttendees, id, user);

            return HandleResult(addResult);
        }

        [HttpPut("{id:int}/edit-attendee")]
        [Authorize]
        [LoadUser]
        //[SubscriptionCheck(Actions.EditAttendees)]
        [CheckEventPermission(Actions.EditAttendees)]
        public async Task<IActionResult> EditAttendees([FromBody] List<EditAttendeesRequestDto> editedAttendees, int id)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;

            var editResult = await _eventRepo.EditAttendeesAsync(editedAttendees, id, user);

            return HandleResult(editResult);
        }
        
        [HttpPost("{id:int}/delete-attendee")]
        [Authorize]
        [LoadUser]
        //[SubscriptionCheck(Actions.DeleteAttendees)]
        [CheckEventPermission(Actions.DeleteAttendees)]
        public async Task<IActionResult> DeleteAttendees([FromBody] List<DeleteAttendeesRequestDto> DeleteAttendees, int id)
        {
            var user = HttpContext.Items["AppUser"] as AppUser;

            var deleteResult = await _eventRepo.DeleteAttendeesAsync(DeleteAttendees, id, user);

            if (!deleteResult.IsSuccess)
            {
                return HandleResult(deleteResult);                
            }

            return NoContent();
        }
        

    }

}
