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

namespace EventApi.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDBContext _context;
        private readonly IFileHandlingService _fileHandle;
        public EventRepository(AppDBContext context, IWebHostEnvironment webHost, IFileHandlingService fileHandle)
        {
            _context = context;
            _fileHandle = fileHandle;
        }

        public async Task<Event> CreateNewEventAsync(NewEventInfo eventInfo)
        {
            var backgroundImageString = await _fileHandle.SaveImageAsync(eventInfo.BackgroundImage);

            var eventModel = eventInfo.EventDetails.ToEventFromEventRequestDto(eventInfo.CreatingUser, backgroundImageString);

            await _context.Events.AddAsync(eventModel);
            await _context.SaveChangesAsync();

            var templateModel = eventInfo.TemplateElements.ToTemplateElementsFromTemplateDto(eventModel.Id);

            await _context.TemplateElements.AddRangeAsync(templateModel);
            await _context.SaveChangesAsync();

            List<Attendee> attendees = await _fileHandle.ParseAttendeesAsync(eventInfo.AttendeeFile);

            foreach (var attendee in attendees)
            {
                attendee.EventId = eventModel.Id;
            }

            await _context.Attendees.AddRangeAsync(attendees);
            await _context.SaveChangesAsync();

            return eventModel;
        }

        public async Task<List<EventSummaryDto>> GetAllUserEventsAsync(AppUser user, EventQueryObject query)
        {
            var userEvents = _context.Events.Where(e => e.AppUserId == user.Id).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                userEvents = userEvents.Where(e => e.Name.Contains(query.Name));
            }

            var sortExpresions = new Dictionary<string, Expression<Func<Event, object>>>
            {
                {"name",e => e.Name},
                {"eventdate",e => e.EventDate}
            };

            var selectedSortExpression = sortExpresions.GetValueOrDefault(query.OrderBy.ToLower(), e => e.EventDate);

            if (!string.IsNullOrWhiteSpace(query.OrderBy))
            {
                userEvents = query.IsDescending ? userEvents.OrderByDescending(selectedSortExpression) : userEvents.OrderBy(selectedSortExpression);
            }

            var skipNumber = (query.PageNumber - 1) * query.PageSize;

            return await userEvents
                .Skip(skipNumber).Take(query.PageSize)
                .Select(u => u.EventToSummaryDto(u.Attendees.Count()))
                .ToListAsync();
        }

        public async Task<EventDetailsDto> GetEventDetailsByIdAsync(int id)
        {
            var eventModel = await _context.Events.Include(e => e.Attendees).Include(e => e.TemplateElements).FirstOrDefaultAsync(e => e.Id == id);

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
    }
}