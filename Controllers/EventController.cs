using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EventApi.DTO.Event;
using EventApi.DTO.Internal;
using EventApi.DTO.Query;
using EventApi.DTO.Template;
using EventApi.Interfaces;
using EventApi.Models;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        public EventController(UserManager<AppUser> userManager, IEventRepository eventrepo, IImageGenerationService imageGen ,IBackgroundJobClient backgroundJob)
        {
            _eventRepo = eventrepo;
            _userManager = userManager;
            _imageGen = imageGen;
            _backgroundJob = backgroundJob;
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

            if (attendeeFile.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                return BadRequest("Invalid file type");


            var firebaseUid = User.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized("Invalid token: Firebase UID is missing.");
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user == null)
            {
                return Unauthorized("User profile does not exist. Please sync first.");
            }
        
            if (await _eventRepo.EventExistsAsync(user.Id, createEventDto))
            {
                return Conflict("An event with this name and date has already been created.");
            }

            var newEventInfo = new NewEventInfo
            {
                EventDetails = createEventDto,
                TemplateElements = templateElementsDto,
                AttendeeFile = attendeeFile,
                BackgroundImage = backgroundImage,
                CreatingUser = user
            };

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


        [HttpGet("all")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<EventSummaryDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllUserEvents([FromQuery] EventQueryObject query)
        {
            var firebaseUid = User.FindFirst("user_id")?.Value;

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
        [ProducesResponseType(500)]
        [ProducesResponseType(200)]
        [Authorize]        
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var eventDetails = await _eventRepo.GetEventDetailsByIdAsync(id);

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
        public async Task<IActionResult> GetDownloadById([FromRoute] int id)
        {
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

    }

}
