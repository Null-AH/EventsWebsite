using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EventApi.DTO.Event;
using EventApi.DTO.Internal;
using EventApi.DTO.Query;
using EventApi.DTO.Template;
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

namespace EventApi.Controllers
{
    [Route("api/event")]
    [ApiController]
    public class EventController : ControllerBase
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
        public async Task<IActionResult> CreateFree([FromForm] string eventInfo, IFormFile attendeeFile)
        {
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            CreateEventDto? createEventDto = JsonSerializer.Deserialize<CreateEventDto>(eventInfo);
            if (createEventDto == null || string.IsNullOrWhiteSpace(createEventDto.Name))
            {
                return BadRequest("Event name is missing or invalid.");
            }
            if (attendeeFile == null || attendeeFile.Length == 0)
                return BadRequest("Attendee file required");

            var allowedFileTypes = new[]
            {
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
                "text/csv" // .csv
            };
            if (!allowedFileTypes.Contains(attendeeFile.ContentType))
            {
                // 3. Return a more helpful error message
                return BadRequest("Invalid file type. Please upload an XLSX or CSV file.");
            }

            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized("Invalid token: Firebase UID is missing.");
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user == null)
            {
                _logger.LogError("PERMISSION CHECK PRE-FAILURE: Could not find user in DB with Firebase UID: {FirebaseUid}. Returning 401.", firebaseUid);
                return Unauthorized("User profile does not exist. Please sync first.");
            }
            _logger.LogInformation("PERMISSION CHECK PRE-STEP: Found user with AppUser ID: {UserId}. Now checking permission in repository.", user.Id);
            // if (await _eventRepo.EventExistsAsync(user.Id, createEventDto))
            // {
            //     return Conflict("An event with this name and date has already been created.");
            // }

            var newEventInfo = new NewEventInfo
            {
                EventDetails = createEventDto,
                TemplateElements = null,
                AttendeeFile = attendeeFile,
                BackgroundImage = null,
                CreatingUser = user
            };

            var createdEvent = await _eventRepo.CreateNewEventAsync(newEventInfo);

            if (createdEvent == null)
                return StatusCode(500, "An error occurred while creating the event.");

            return Ok();
        }


        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] string eventInfo, [FromForm] string templateInfo, IFormFile attendeeFile, IFormFile backgroundImage)
        {

            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            CreateEventDto? createEventDto = JsonSerializer.Deserialize<CreateEventDto>(eventInfo);
            List<TemplateElementsDto>? templateElementsDto = JsonSerializer.Deserialize<List<TemplateElementsDto>>(templateInfo);

            if (createEventDto == null || string.IsNullOrWhiteSpace(createEventDto.Name))
            {
                return BadRequest("Event name is missing or invalid.");
            }
            if (templateElementsDto == null || !templateElementsDto.Any())
            {
                return BadRequest("Template elements are required.");
            }
            if (templateElementsDto.Any(t => string.IsNullOrWhiteSpace(t.ElementType)))
            {
                return BadRequest("One or more template elements has a missing or invalid type.");
            }


            if (backgroundImage == null || backgroundImage.Length == 0)
                return BadRequest("Background image is required.");

            var allowedImageTypes = new[] { "image/jpeg", "image/png" };

            if (!allowedImageTypes.Contains(backgroundImage.ContentType))
                return BadRequest("Invalid image file type. Please upload a JPG or PNG.");

            if (attendeeFile == null || attendeeFile.Length == 0)
                return BadRequest("Attendee file required");


            var allowedFileTypes = new[]
                {
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
                    "text/csv" // .csv
                };

            // 2. Check if the file's type is NOT in our allowed list
            if (!allowedFileTypes.Contains(attendeeFile.ContentType))
            {
                // 3. Return a more helpful error message
                return BadRequest("Invalid file type. Please upload an XLSX or CSV file.");
            }

            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized("Invalid token: Firebase UID is missing.");
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user == null)
            {
                return Unauthorized("User profile does not exist. Please sync first.");
            }

            // if (await _eventRepo.EventExistsAsync(user.Id, createEventDto))
            // {
            //     return Conflict("An event with this name and date has already been created.");
            // }

            var newEventInfo = new NewEventInfo
            {
                EventDetails = createEventDto,
                TemplateElements = templateElementsDto,
                AttendeeFile = attendeeFile,
                BackgroundImage = backgroundImage,
                CreatingUser = user
            };

            try
            {

                var createdEvent = await _eventRepo.CreateNewEventAsync(newEventInfo);

                if (createdEvent == null)
                    return StatusCode(500, "An error occurred while creating the event.");

                _backgroundJob.Enqueue<IImageGenerationService>(service => service.GenerateInvitationsForEventAsync(createdEvent.Id));

                //var zipFilePath = await _imageGen.GenerateInvitationsForEventAsync(createdEvent.Id);
                //var fileBytes = await System.IO.File.ReadAllBytesAsync(zipFilePath);
                //System.IO.File.Delete(zipFilePath);
                //return File(fileBytes, "application/zip", "invitations.zip",enableRangeProcessing : true);

                return AcceptedAtAction(nameof(GetById), new { id = createdEvent.Id }, createdEvent);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected internal error occurred.");
            }

        }


        [HttpGet("all")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<EventSummaryDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllUserEvents([FromQuery] EventQueryObject query)
        {
            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized("Invalid token: Firebase UID is missing.");
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user == null)
            {
                return Unauthorized("User profile does not exist. Please sync first.");
            }

            var userEvents = await _eventRepo.GetAllUserEventsAsync(user, query);

            return Ok(userEvents);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        [ProducesResponseType(500)]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]

        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Invalid token: Firebase UID is missing.");
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return Unauthorized("User profile does not exist. Please sync first.");

            await _eventRepo.CheckPermissionAsync(user, id, Actions.EventGetById);

            var eventDetails = await _eventRepo.GetEventDetailsByIdAsync(id, user);

            if (eventDetails == null)
            {
                return NotFound("Failed to Fetch Event Details");
            }

            return Ok(eventDetails);
        }

        [HttpGet("{id:int}/download")]
        [ProducesResponseType(500)]
        [ProducesResponseType(200)]
        [ProducesResponseType(202)]
        [ProducesResponseType(401)]

        public async Task<IActionResult> GetDownloadById([FromRoute] int id)
        {
            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Invalid token: Firebase UID is missing.");
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return Unauthorized("User profile does not exist. Please sync first.");

            await _eventRepo.CheckPermissionAsync(user, id, Actions.EventDownloadZip);

            var eventInfo = await _eventRepo.GetEventByIdAsync(id);

            if (eventInfo == null)
            {
                return NotFound("Event not found.");
            }

            if (string.IsNullOrEmpty(eventInfo.GeneratedInvitationsZipUri))
            {
                return Accepted("Processing... Please Check later");
            }

            return Ok(eventInfo.GeneratedInvitationsZipUri);
        }

        [HttpPut("edit")]
        [Authorize]

        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        [ProducesResponseType(200)]

        public async Task<IActionResult> UpdateEvent([FromBody] UpdateEventRequestDto updateEventRequestDto)
        {
            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Invalid token: Firebase UID is missing.");
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return Unauthorized("User profile does not exist. Please sync first.");

            await _eventRepo.CheckPermissionAsync(user, updateEventRequestDto.Id, Actions.EventEdit);

            if (!ModelState.IsValid) return BadRequest(ModelState);
            var editedEvent = await _eventRepo.UpdateEvent(updateEventRequestDto.Id, updateEventRequestDto, user);
            if (editedEvent == null)
            {
                return NotFound("Event Do not exist");
            }

            return Ok(editedEvent);
        }

        [HttpDelete("delete")]
        [Authorize]

        [ProducesResponseType(404)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]

        public async Task<IActionResult> DeleteEvent([FromQuery] int id)
        {
            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Invalid token: Firebase UID is missing.");
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return Unauthorized("User profile does not exist. Please sync first.");

            await _eventRepo.CheckPermissionAsync(user, id, Actions.EventDelete);

            var deletedEvent = await _eventRepo.DeleteEventByIdAsync(id);
            if (deletedEvent == null)
            {
                return NotFound("Event Do not exist");
            }
            return NoContent();
        }


        [HttpPost("{id:int}/check-in")]
        [Authorize]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> QrCheckIn([FromRoute] int id, [FromBody] EventCheckInRequestDto checkInRequestDto)
        {
            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized("Invalid token: Firebase UID is missing.");
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user == null)
            {
                return Unauthorized("User profile does not exist. Please sync first.");
            }

            await _eventRepo.CheckPermissionAsync(user, id, Actions.CheckIn);

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
        public async Task<IActionResult> GetCount(int id)
        {
            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value; if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized();
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return Unauthorized();

            await _eventRepo.CheckPermissionAsync(user, id, Actions.Count);


            var count = await _eventRepo.GetCurrentCheckedInCountAsync(id, user.Id);

            if (count == null)
            {
                return NotFound("Event not found or you do not have permission to view it.");
            }

            return Ok(new { CheckedInCount = count });
        }

        [HttpPost("{id:int}/addteam")]
        [Authorize]

        public async Task<IActionResult> AddCollaborators([FromBody] List<AddCollaboratorsRequestDto> addCollaboratorsDto, int id)
        {
            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Invalid token: Firebase UID is missing.");
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return Unauthorized("User profile does not exist. Please sync first.");

            await _eventRepo.CheckPermissionAsync(user, id, Actions.AddCollaborators);

            await _eventRepo.AddCollaboratorsAsync(addCollaboratorsDto, user, id);

            return Ok("Almost Done!");
        }

        [HttpPut("{id:int}/editteam")]
        [Authorize]

        public async Task<IActionResult> EditCollaborators([FromBody] List<EditCollaboratorRequestDto> editCollaboratorsDto, int id)
        {
            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Invalid token: Firebase UID is missing.");
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return Unauthorized("User profile does not exist. Please sync first.");

            await _eventRepo.CheckPermissionAsync(user, id, Actions.EditCollaborators);

            var editedCollaborator = await _eventRepo.EditCollaboratorsAsync(editCollaboratorsDto, user, id);

            return Ok(editedCollaborator);
        }

        [HttpGet("{id:int}/getteam")]
        [Authorize]
        public async Task<IActionResult> GetCollaborators(int id)
        {
            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Invalid token: Firebase UID is missing.");
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return Unauthorized("User profile does not exist. Please sync first.");

            await _eventRepo.CheckPermissionAsync(user, id, Actions.GetCollaborators);

            var collaboratorsResponseDto = await _eventRepo.GetCollaboratorsAsync(id, user.Id);

            return Ok(collaboratorsResponseDto);
        }

        [HttpPost("{id:int}/deleteteam")]
        [Authorize]
        public async Task<IActionResult> DeleteCollaborators([FromRoute] int id, [FromBody] List<string> userToDeleteId)
        {
            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Invalid token: Firebase UID is missing.");
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return Unauthorized("User profile does not exist. Please sync first.");

            await _eventRepo.CheckPermissionAsync(user, id, Actions.DeleteCollaborators);

            var success = await _eventRepo.DeleteCollaboratorsAsync(id, user, userToDeleteId);

            if (!success)
            {
                return BadRequest("Collaborator could not be removed, They might be the event owner or do not exist");
            }

            return NoContent();
        }

        [HttpPost("{id:int}/leave")]
        [Authorize]
        public async Task<IActionResult> LeaveEvent([FromRoute] int id, [FromBody] string userId)
        {
            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Invalid token: Firebase UID is missing.");
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return Unauthorized("User profile does not exist. Please sync first.");

            await _eventRepo.CheckPermissionAsync(user, id, Actions.DeleteCollaborators);
            bool success = await _eventRepo.LeaveEventAsync(id, user);

            if (!success)
            {
                return BadRequest("Collaborator could not be removed, They might be the event owner or do not exist");
            }

            return NoContent();
        }

        


    }

}
