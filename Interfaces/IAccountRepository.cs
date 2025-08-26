using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.ExeptionHandling;
using EventApi.Models;

namespace EventApi.Interfaces
{
    public interface IAccountRepository
    {
        Task<Result<AppUser>> SyncUserAsync(string firebaseUid, string email, string? userName, string? picture);
    }
}