using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EventApi.Interfaces;
using EventApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace EventApi.Filters
{
    public class LoadUserAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            var eventRepo = context.HttpContext.RequestServices.GetRequiredService<IEventRepository>();
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();

            var firebaseUid = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(firebaseUid))
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "User.NotAuthenticated",
                    Detail = "User is not authenticated."
                };
                context.Result = new ObjectResult(problemDetails) { StatusCode = 401 };
                return;
            }
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "User.NotAuthenticated",
                    Detail = "User profile does not exist or token is invalid."
                };
                context.Result = new ObjectResult(problemDetails) { StatusCode = 401 };
                return;
            }

            context.HttpContext.Items["AppUser"] = user;

            await next();
        }
    }
}