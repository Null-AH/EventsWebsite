using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.DTO.Internal;
using EventApi.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace EventApi.Controllers
{
    [ApiController]
    [Route("api/Webhooks")]
    public class WebhooksController : BaseApiController
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<WebhooksController> _logger;
        public WebhooksController(IBackgroundJobClient backgroundJobClient, ILogger<WebhooksController> logger)
        {
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        [HttpPost("whatsapp-status")]
        public async Task<IActionResult> HandleWhatsAppStatus()
        {
            string rawJsonBody;
            using (var reader = new StreamReader(Request.Body))
            {
                rawJsonBody = await reader.ReadToEndAsync();
            }

            _logger.LogInformation("----------- RAW WEBHOOK RECEIVED ------------- \n{Payload}", rawJsonBody);

            try
            {
                var responseUpdateDto = System.Text.Json.JsonSerializer.Deserialize<WasenderWebHookResponseDto>(rawJsonBody);
                _logger.LogInformation("Webhook received: {Payload}", responseUpdateDto);

                if (responseUpdateDto?.Data?.AlphanumericId != null &&
                    responseUpdateDto.Data.Key.FromMe &&
                    responseUpdateDto.Data.NumericMessageId != null &&
                    responseUpdateDto.Data.Status != null)
                {
                    var alphanumericId = responseUpdateDto.Data.AlphanumericId;
                    var numericMessageId = responseUpdateDto.Data.NumericMessageId.Value;
                    var status = responseUpdateDto.Data.Status.Value;

                    _backgroundJobClient.Enqueue<IMessageService>(service => service.LinkIdAndUpdateStatusAsync(numericMessageId, alphanumericId, status));

                    return Ok();
                }

                else if (responseUpdateDto?.Data?.Key?.MessageId != null &&
                    responseUpdateDto.Data.Status != null)
                {
                    var messageId = responseUpdateDto.Data.Key.MessageId;
                    var status = responseUpdateDto.Data.Status.Value;

                    _backgroundJobClient.Enqueue<IMessageService>(service => service.UpdateMessageStatusAsync(messageId, status));

                    _logger.LogInformation("--------------------------Webhook processed for MessageId: {MessageId} with Status: {Status}--------------------------", messageId, status);
                    return Ok();
                }
                else
                {
                    _logger.LogInformation($"--------------------------Webhook event was wrong: {responseUpdateDto?.Event}--------------------------");
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize webhook payload.");
            }
            return Ok();
        }
    }
}