using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.Interfaces
{
    public interface IFirebaseAdminService
    {
        Task<string> GetEmailFromOobCodeAsync(string oobCode);
    }
}