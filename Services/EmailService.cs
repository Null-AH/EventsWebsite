using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using EventApi.Interfaces;
using EventApi.Models;
using Hangfire;
using MailerSendNetCore.Common.Interfaces;
using MailerSendNetCore.Emails.Dtos;
using Microsoft.AspNetCore.Authentication;
using Org.BouncyCastle.Cms;



namespace EventApi.Services
{
    public class EmailService : IEmailSevice
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _frontendBaseUrl;
        private readonly IMailerSendEmailClient _mailerSendClient;
        

        public EmailService(IConfiguration config, ILogger<EmailService> logger, IBackgroundJobClient backgroundJobClient
        , IMailerSendEmailClient mailerSendEmailClient)
        {
            _config = config;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            var apiKey = config["EmailSettings:MailerSendApiKey"];
            _senderEmail = config["EmailSettings:SenderEmail"];
            _senderName = config["EmailSettings:SenderName"];
            _frontendBaseUrl = config["Frontend:BaseUrl"];

            // Initialize the MailerSend client with your API key
            _mailerSendClient = mailerSendEmailClient;
        }

        public Task SendCollaboratorInvitationEmailAsync
        (string collaboratorEmail, string eventName, Role invitedRole, string invitationToken)
        {
            var frontendAcceptUrl = $"https://your-frontend-app.com/accept-invitation?token={invitationToken}";

            _logger.LogInformation("---------------------------- � SIMULATING EMAIL � ------------------------------");
            _logger.LogInformation("To: {RecipientEmail}", collaboratorEmail);
            _logger.LogInformation("From: \"Qr Platform\" <noreply@your-app-domain.com>");
            _logger.LogInformation("Subject: You've been invited to collaborate on {EventName}!", eventName);
            _logger.LogInformation("Body: You have been invited as an '{InvitedRole}' to the event '{EventName}'. Click the link to accept: {AcceptLink}", invitedRole, eventName, frontendAcceptUrl);
            _logger.LogInformation("---------------------------------");

            // We return Task.CompletedTask because this operation is instant.
            return Task.CompletedTask;
        }

        public async Task SendVerificationEmailAsync(string userEmail, string oobCode)
        {
            var frontendVerificationUrl = $"{_frontendBaseUrl}/verify-email?oobCode={oobCode}";

            var emailParams = new MailerSendEmailParameters()
            .WithFrom(_senderEmail, _senderName)
            .WithTo(userEmail)
            .WithSubject("Verify your email for QR Platform")
            .WithHtmlBody($"<p>Please <a href=\"{frontendVerificationUrl}\">Click Here to Verify</a></p>"); 

            try
            {
                var response = await _mailerSendClient.SendEmailAsync(emailParams);

                _logger.LogInformation("Successfully sent verification email to {RecipientEmail} via MailerSend.", userEmail);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while sending verification email via MailerSend.");
            }
        }
    }
}