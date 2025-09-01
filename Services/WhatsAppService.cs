using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using EventApi.Data;
using EventApi.DTO.Internal;
using EventApi.ExeptionHandling;
using EventApi.Interfaces;
using EventApi.Models;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.Record.CF;
using NPOI.HSSF.Record.Chart;
using NPOI.OpenXmlFormats.Wordprocessing;

namespace EventApi.Services
{
    public class WhatsAppService : IMessageService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _webhost;
        private readonly string? appBaseUrl;
        private readonly string? apiKey;
        private readonly string? senderPhone;
        private readonly string? senderName;
        public WhatsAppService(IConfiguration config, IHttpClientFactory httpClient
        , ILogger<WhatsAppService> logger, AppDBContext context, IWebHostEnvironment webhost)
        {
            _context = context;
            _webhost = webhost;
            _httpClientFactory = httpClient;
            _logger = logger;
            _config = config;
            appBaseUrl = config["Backend:AppBaseUrl"];
            apiKey = config["WhatsAppSettings:WasenderApiKey"];
            senderPhone = config["WhatsAppSettings:SenderPhone"];
            senderName = config["WhatsAppSettings:SenderName"];
        }
        public async Task<Result<string>> SendAttendeeInvitationWhatsAppAsync(int attendeeId)
        {
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appBaseUrl))
            {
                return Result<string>.Failure(ConfigutationErrors.ConfigurationError);
            }

            var attendee = await _context.Attendees.Include(e => e.Event).FirstOrDefaultAsync(e => e.Id == attendeeId);
            if (attendee == null)
            {
                return Result<string>.Failure(AttendeeErrors.NotFound);
            }

            var downloadsRelativePath = Path.Combine("/downloads/generated_invitations/");

            var safeAttendeeName = string.Join("_", attendee.Name.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"{safeAttendeeName}_{attendee.Id}.jpg";

            var imageRelativePath = Path.Combine(downloadsRelativePath, "احمد حازم عباس_16483.jpg");

            var publicImageUrl = $"{appBaseUrl}/{imageRelativePath}";


            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var wasenderEndpoint = "https://wasenderapi.com/api/send-message";
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var response = await httpClient.PostAsJsonAsync(wasenderEndpoint, new
                {
                    to = attendee.PhoneNumber,
                    text = $"Hello {attendee.Name}, You have been invited to the {attendee.Event.Name} event",
                    imageUrl = publicImageUrl
                });

                if (response.IsSuccessStatusCode)
                {
                    var responseDto = await response.Content.ReadFromJsonAsync<WasenderSendSuccessResponse>();

                    if (responseDto?.Data != null)
                    {
                        attendee.MessageId = responseDto.Data.MessageId.ToString();
                        attendee.InvitationStatus = DeliveryStatus.Sent;
                        await _context.SaveChangesAsync();
                        return Result<string>.Success("Message Sent!, or check the dashboard");
                    }
                    else
                    {
                        _logger.LogError("Wasender API returned a successful status but the response body was invalid.");
                        return Result<string>.Success("Message Sent!, or check the dashboard");
                    }
                }
                else
                {
                    var errorContent = response.Content.ReadAsStringAsync();
                    _logger.LogError("Wasender API returned a failure: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    return Result<string>.Failure(ApiCallErrors.WhatsAppWasenderError);
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while calling the Wasender API.");
                return Result<string>.Failure(ApiCallErrors.WhatsAppWasenderError);
            }
        }

        public async Task UpdateMessageStatusAsync(string messageId, int newStatus)
        {
            var attendee = await _context.Attendees.FirstOrDefaultAsync(a => a.MessageId == messageId);

            if (attendee == null)
            {
                _logger.LogWarning("---------------------------Received webhook status for unknown MessageId: {MessageId}---------------------------", messageId);
                return;
            }

            attendee.InvitationStatus = (DeliveryStatus)newStatus;
            await _context.SaveChangesAsync();
            return;
        }
        public async Task LinkIdAndUpdateStatusAsync(long numericMessageId, string alphanumericId, int newStatus)
        {
            var attendeeNormal = await _context.Attendees.FirstOrDefaultAsync(a => a.MessageId == numericMessageId.ToString());
            var attendeeBackup = await _context.Attendees.FirstOrDefaultAsync(a => a.MessageId == alphanumericId);
            if (attendeeNormal == null && attendeeBackup == null)
            {
                _logger.LogWarning("---------------------------Received webhook(Linking) status for unknown MessageId: {MessageId}---------------------------", numericMessageId);
                return;
            }
            var attendee = attendeeNormal ?? attendeeBackup;

            attendee.MessageId = alphanumericId;
            attendee.InvitationStatus = (DeliveryStatus)newStatus;
            await _context.SaveChangesAsync();
            return;
        }
    }
}