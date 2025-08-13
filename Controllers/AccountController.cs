using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using EventApi.Data;
using EventApi.DTO.Account;
using EventApi.Models;
using Google.Apis.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Wordprocessing;

namespace api.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDBContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<AppUser> userManager, ILogger<AccountController> logger, AppDBContext context)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [HttpPost("sync")]
        [Authorize]
        public async Task<IActionResult> SyncUser()
        {
            var claimsData = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
            var claims = JsonSerializer.Serialize(claimsData, new JsonSerializerOptions { WriteIndented = true });

            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var userName = User.FindFirst("name")?.Value;
            var picture = User.FindFirst("picture")?.Value;

             _logger.LogInformation("--- SYNC START ---");
            _logger.LogInformation("Sync request for Firebase UID: {FirebaseUid}, Email: {Email}", firebaseUid, email);

            if (string.IsNullOrEmpty(firebaseUid) || string.IsNullOrEmpty(email))
            {
                _logger.LogError("Sync failed: Token is missing UID or Email.");
                return BadRequest($"Invalid token information from Firebase.\nUsername : {userName}\nEmail:{email}\n{claims}");
            }

            AppUser userToProcess;

            var existingUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid || u.NormalizedEmail == email.ToUpperInvariant());

            if (existingUser == null)
            {
                 _logger.LogInformation("User not found in DB. Creating new user.");
                char initialChar = !string.IsNullOrEmpty(userName) ? userName[0] : email[0];
                string placeHolderLetter = char.ToUpper(initialChar).ToString();
                var newUser = new AppUser
                {

                    UserName = firebaseUid,
                    Email = email,
                    FirebaseUid = firebaseUid,
                    PictureUrl = picture ?? $"https://placehold.co/600x400/2B2543/FFFFFF/png?text={System.Net.WebUtility.UrlEncode(placeHolderLetter)}",
                    DisplayName = userName
                };

                var result = await _userManager.CreateAsync(newUser);

                if (!result.Succeeded)
                {
                     _logger.LogError("Failed to create new user. Errors: {Errors}", result.Errors);
                    return StatusCode(500, result.Errors);
                }
                userToProcess = newUser;
                _logger.LogInformation("New user created successfully. AppUser ID: {UserId}", userToProcess.Id);
            }
            else
            {
                 _logger.LogInformation("Found existing user with AppUser ID: {UserId}. Checking for updates.", existingUser.Id);
                bool needsUpdate = false;
                if (existingUser.FirebaseUid != firebaseUid)
                {
                    _logger.LogWarning("Existing user was found by email but is MISSING FirebaseUid. Updating UID from {OldUid} to {NewUid}", existingUser.FirebaseUid, firebaseUid);
                    existingUser.FirebaseUid = firebaseUid;
                    needsUpdate = true;
                }
                if (existingUser.UserName != firebaseUid)
                {
                    _logger.LogWarning("Existing user was found by email but is MISSING UserName. Updating UserName from {OldUid} to {NewUid}", existingUser.UserName, firebaseUid);
                    existingUser.UserName = firebaseUid;
                    needsUpdate = true;
                }
                if (existingUser.PictureUrl != picture && picture != null)
                {
                    existingUser.PictureUrl = picture;
                    needsUpdate = true;
                }
                if (existingUser.DisplayName != userName)
                {
                    existingUser.DisplayName = userName;
                    needsUpdate = true;
                }
                if (needsUpdate)
                {
                    _logger.LogInformation("Updating user profile.");
                    var updateResult = await _userManager.UpdateAsync(existingUser);
                    if (!updateResult.Succeeded)
                    {
                        return StatusCode(500, "Failed to update user profile during sync");
                    }
                }
                userToProcess = existingUser;
            }


            _logger.LogInformation("Processing invitations for AppUser ID: {UserId} with Email: {UserEmail}", userToProcess.Id, userToProcess.Email);

            var pendingInvitedUsers = await _context.CollaboratorsInvitations
            .Where(e => e.InvitedEmail == userToProcess.Email && e.Status == Status.Pending).ToListAsync();

            if (pendingInvitedUsers.Any())
            {
                _logger.LogInformation("Found {Count} pending invitations.", pendingInvitedUsers.Count);
                foreach (var invitation in pendingInvitedUsers)
                {
                    _logger.LogInformation("Processing invitation for Event ID: {EventId}. Creating EventCollaborators link for AppUser ID: {UserId}", invitation.EventId, userToProcess.Id);

                    var alreadyCollaborator = await _context.EventCollaborators
                    .AnyAsync(ec => ec.EventId == invitation.EventId &&
                    ec.UserId == userToProcess.Id);

                    if (!alreadyCollaborator)
                    {
                        await _context.EventCollaborators.AddAsync(
                            new EventCollaborators
                            {
                                UserId = userToProcess.Id,
                                EventId = invitation.EventId,
                                Role = invitation.Role
                            });
                        invitation.Status = Status.Deleted;
                    }
                }
                

                await _context.SaveChangesAsync();
                 _logger.LogInformation("Saved changes for invitations.");
            }
              else
            {
                _logger.LogInformation("No pending invitations found for this user.");
            }
             _logger.LogInformation("--- SYNC END ---");
            return Ok("User synced successfully.");
        }
    }
}