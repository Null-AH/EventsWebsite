using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.ExeptionHandling;
using EventApi.Models;
using NPOI.SS.Formula.Functions;

namespace EventApi.Interfaces
{
    public interface IMessageService
    {
        Task<Result<string>> SendAttendeeInvitationWhatsAppAsync(int attendeeId);
        Task UpdateMessageStatusAsync(string messageId, int status);
        Task LinkIdAndUpdateStatusAsync(long numericMessageId, string alphanumericId, int newStatus);

    }
}