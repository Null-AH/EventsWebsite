using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EventApi.Helpers;
using EventApi.Interfaces;
using EventApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace EventApi.Filters
{
    public class SubscriptionCheckAttribute : Attribute, IAsyncActionFilter
    {
        private Actions _action;
        
        public SubscriptionCheckAttribute(Actions action)
        {
            _action = action;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var eventRepo = context.HttpContext.RequestServices.GetRequiredService<IEventRepository>();
            //var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();

            var user = context.HttpContext.Items["AppUser"] as AppUser;

            if (user == null)
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server.ConfigurationError",
                    Detail = "User object was not found in the request context."
                };
                context.Result = new ObjectResult(problemDetails) { StatusCode = 500 };
                return;
            }
            var result = await eventRepo.CheckSubscriptionAsync(user, _action);


            if (result.IsSuccess)
            {
                await next();
            }
            else
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = result.Error.Code,
                    Detail = result.Error.Description
                };
                context.Result = new ObjectResult(problemDetails)
                {
                    StatusCode = 403
                };
            }

        }
    }
}