using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using EventApi.Controllers;
using EventApi.Data;
using EventApi.DTO.Account;
using EventApi.Interfaces;
using EventApi.Models;
using Google.Apis.Util;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.SS.Formula.Functions;

namespace EventApi.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDBContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly IFirebaseAdminService _firebaseAdmin;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IAccountRepository _accountRepo;

        public AccountController
        (UserManager<AppUser> userManager, ILogger<AccountController> logger, AppDBContext context
        , IFirebaseAdminService firebaseAdmin, IBackgroundJobClient backgroundJobClient,
        IAccountRepository accountRepository)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _firebaseAdmin = firebaseAdmin;
            _backgroundJobClient = backgroundJobClient;
            _accountRepo = accountRepository;
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

            if (string.IsNullOrEmpty(firebaseUid) || string.IsNullOrEmpty(email))
            {
                //_logger.LogError("Sync failed: Token is missing UID or Email.");
                return new ObjectResult(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "User.Validation",
                    Detail =$"Invalid token information from Firebase.\nUsername : {userName}\nEmail:{email}\n{claims}"
                });
            }
            var result = await _accountRepo.SyncUserAsync(firebaseUid, email, userName, picture);

            if(!result.IsSuccess)
                return HandleResult(result);

            _logger.LogInformation("--- SYNC END ---");

            return Ok("User synced successfully.");

        }

        [HttpGet("verify")]
        [AllowAnonymous]
        public async Task<IActionResult> FirebaseVerification([FromQuery] string oobCode, [FromQuery] string mode)
        {

            if (string.IsNullOrEmpty(oobCode))
            {
                return BadRequest("Action code is required.");
            }

            if (mode == "verifyEmail")
            {
                var userEmail = await _firebaseAdmin.GetEmailFromOobCodeAsync(oobCode);

                if (userEmail == null)
                {
                    return Redirect("https://frontend/link-expired");
                }

                _backgroundJobClient.Enqueue<IEmailSevice>(service => service.SendVerificationEmailAsync(
                    userEmail, oobCode
                ));

                return Redirect("https://frontend/check-your-email");
            }

            return BadRequest("Invalid action mode.");

        }
    }
}