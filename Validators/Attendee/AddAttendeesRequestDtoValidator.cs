using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using EventApi.DTO.AttendeeDTO;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text.Json;

namespace EventApi.Validators.Attendee
{
    public class AddAttendeesRequestDtoValidator : AbstractValidator<AddAttendeesRequestDto>
    {
        public AddAttendeesRequestDtoValidator()
        {

            RuleFor(a => a.Name).NotNull().NotEmpty()
            .WithMessage("The Attendee's name is required");
            
            RuleFor(a => a.Email).NotNull().NotEmpty()
            .WithMessage("The Attendee's Email is required");


        }
    }
}