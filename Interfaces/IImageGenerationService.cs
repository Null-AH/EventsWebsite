using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.Interfaces
{
    public interface IImageGenerationService
    {
        public Task/*<string>*/ GenerateInvitationsForEventAsync(int eventId);
    }
}