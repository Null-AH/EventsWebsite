using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.ExeptionHandling;
using Microsoft.AspNetCore.Mvc;

namespace EventApi.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        private ProblemDetails CreateProblemDetails(Error error, int statusCode)
        {
            return new ProblemDetails
            {
                Status = statusCode,
                Title = error.Code,
                Detail = error.Description
            };
        }
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return FailureHandler(result.Error);        
        }
        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
            {
                return Ok();
            }

            return FailureHandler(result.Error);
        }

        private IActionResult FailureHandler(Error error)
        {
            var errorCodeType = error.Code.Split('.').LastOrDefault();

            switch (errorCodeType)
            {
                case "NotFound":
                    return NotFound(CreateProblemDetails(error, StatusCodes.Status404NotFound));

                case "Validation":
                    return BadRequest(CreateProblemDetails(error, StatusCodes.Status400BadRequest));

                case "Conlflict":
                    return Conflict(CreateProblemDetails(error, StatusCodes.Status409Conflict));

                case "LimitReached":
                case "SubscriptionLimitReached":
                case "Forbidden":
                    return new ObjectResult(CreateProblemDetails(error, StatusCodes.Status403Forbidden));

                default:
                    return BadRequest(CreateProblemDetails(error, StatusCodes.Status400BadRequest));
            }
        }


    }
    

}