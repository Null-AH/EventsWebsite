using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.Models;

namespace EventApi.Interfaces
{
    public interface IFileHandlingService
    {
        public Task<string> SaveImageAsync(IFormFile imageFile);

        public Task<List<Attendee>> ParseAttendeesAsync(IFormFile attendeeFile);

    }
}