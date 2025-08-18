using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.Models;

namespace EventApi.Interfaces
{
    public interface IEmailSevice
    {
        public Task SendCollaboratorInvitationEmailAsync
        (string collaboratorEmail, string eventName, Role role, string InvitationToken);
        public Task SendVerificationEmailAsync(string userEmail, string oobCode);
    }
}