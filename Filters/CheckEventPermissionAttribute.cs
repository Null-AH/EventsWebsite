using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.ExeptionHandling;
using EventApi.Helpers;
using EventApi.Interfaces;
using EventApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NPOI.OpenXmlFormats.Spreadsheet;

namespace EventApi.Filters
{
    public class CheckEventPermissionAttribute : Attribute,IAsyncActionFilter
    {
        private Actions _action;
        public CheckEventPermissionAttribute(Actions action)
        {
            _action = action;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var eventRepo = context.HttpContext.RequestServices.GetRequiredService<IEventRepository>();
            var user = context.HttpContext.Items["AppUser"] as AppUser;

            //int eventId;
            //if (context.ActionArguments.ContainsKey("id"))
            //{
            //     var eventIdObject = context.ActionArguments["id"];
            //     if (eventIdObject is int)
            //     {
            //         eventId = (int)eventIdObject;
            //     }
            //}
            if (!context.ActionArguments.TryGetValue("id", out var eventIdObject) || !(eventIdObject is int eventId))
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server.ConfigurationError",
                    Detail = "Could not find 'id' parameter in the request for permission check."
                };
                context.Result = new ObjectResult(problemDetails) { StatusCode = 500 };
                return;
            }

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

            var result = await eventRepo.CheckPermissionAsync(user, eventId, _action);

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