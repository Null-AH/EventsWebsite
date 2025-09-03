using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using brevo_csharp.Model;
using EventApi.Data;
using EventApi.ExeptionHandling;
using EventApi.Interfaces;
using EventApi.Models;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SubscriptionTier = EventApi.Models.SubscriptionTier;

namespace EventApi.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDBContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AccountRepository> _logger;

        public AccountRepository(AppDBContext context, UserManager<AppUser> userManager, ILogger<AccountRepository> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }
        public async Task<Result<AppUser>> SyncUserAsync(string firebaseUid, string email, string? userName, string? picture)
        {
            var existingUser = await _userManager.Users
           .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid || u.NormalizedEmail == email.ToUpperInvariant());
            AppUser userToProcess;
            if (existingUser == null)
            {
                //_logger.LogInformation("User not found in DB. Creating new user.");
                char initialChar = !string.IsNullOrEmpty(userName) ? userName[0] : email[0];
                string placeHolderLetter = char.ToUpper(initialChar).ToString();
                var newUser = new AppUser
                {
                    UserName = firebaseUid,
                    Email = email,
                    FirebaseUid = firebaseUid,
                    PictureUrl = picture ?? $"https://placehold.co/600x400/2B2543/FFFFFF/png?text={System.Net.WebUtility.UrlEncode(placeHolderLetter)}",
                    DisplayName = userName,
                    Tier = SubscriptionTier.Pro
                };

                var result = await _userManager.CreateAsync(newUser);

                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create new user. Errors: {Errors}", result.Errors);

                    var errors = string.Join(",", result.Errors.Select(e => e.Description));
                    return Result<AppUser>.Failure(new Error(UserErrors.CreationFailed.Code, errors));
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
                    //_logger.LogWarning("Existing user was found by email but is MISSING UserName. Updating UserName from {OldUid} to {NewUid}", existingUser.UserName, firebaseUid);
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
                    //_logger.LogInformation("Updating user profile.");
                    var updateResult = await _userManager.UpdateAsync(existingUser);
                    if (!updateResult.Succeeded)
                    {
                        var errors = string.Join(",", updateResult.Errors.Select(e => e.Description));
                        return Result<AppUser>.Failure(new Error(UserErrors.UpdateFailed.Code, errors));
                    }
                }
                userToProcess = existingUser;
            }

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
                   
            return Result<AppUser>.Success(userToProcess);
        }
    }
}