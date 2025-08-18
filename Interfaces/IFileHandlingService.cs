using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.ExeptionHandling;
using EventApi.Models;

namespace EventApi.Interfaces
{
    public interface IFileHandlingService
    {
        public Task<Result<string>> SaveImageAsync(IFormFile imageFile);

        public Task<Result<List<Attendee>>> ParseAttendeesAsync(IFormFile attendeeFile);

    }
}