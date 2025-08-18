using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EventApi.DTO.Event;
using FluentValidation;

namespace EventApi.Validators.Event
{
    public class CreateEventFreeDtoValidator : AbstractValidator<CreateEventRequestDto>
    {
        public CreateEventFreeDtoValidator()
        {
            RuleFor(x => x.EventInfo).NotEmpty().WithMessage("Event information is required.")
            .Custom((eventInfoJson, context) =>
            {
                if (string.IsNullOrEmpty(eventInfoJson)) return;
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var eventDto = JsonSerializer.Deserialize<CreateEventDto>(eventInfoJson, options);
                    if (eventDto == null || string.IsNullOrWhiteSpace(eventDto.Name))
                    {
                        context.AddFailure("EventInfo.Name", "Event name is missing or invalid within the provided event information.");

                    }
                }
                catch (JsonException)
                {
                    context.AddFailure("EventInfo", "The provided event information is not a valid JSON string.");
                }
            });

            RuleFor(x => x.AttendeeFile)
                .NotNull().WithMessage("Attendee file is required.");
            
            RuleFor(x => x.AttendeeFile.ContentType)
                .Must(ct => ct == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" || ct == "text/csv")
                .When(x => x.AttendeeFile != null) 
                .WithMessage("Invalid attendee file type. Please upload an XLSX or CSV file.");


        }
    }
}