using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using EventApi.Data;
using EventApi.DTO.Account;
using EventApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public AccountController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;

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

            if (string.IsNullOrEmpty(firebaseUid) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userName)||string.IsNullOrEmpty(picture))
            {
                return BadRequest($"Invalid token information from Firebase.\nUsername : {userName}\nEmail:{email}\n{claims}");
            }

            var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (existingUser == null)
            {
                var newUser = new AppUser
                {

                    UserName = firebaseUid,
                    Email = email,
                    FirebaseUid = firebaseUid,
                    PictureUrl = picture,
                    DisplayName = userName
                };

                var result = await _userManager.CreateAsync(newUser);

                if (!result.Succeeded)
                {

                    return StatusCode(500, result.Errors);
                }
            }
            else
            {
                if (existingUser.PictureUrl != picture)
                {
                    existingUser.PictureUrl = picture;
                    await _userManager.UpdateAsync(existingUser);
                }
                if (existingUser.UserName != userName)
                {
                    existingUser.UserName = userName;
                    await _userManager.UpdateAsync(existingUser);
                }
            }

            return Ok("User synced successfully.");
        }
    }
}