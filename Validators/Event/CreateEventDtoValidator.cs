using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EventApi.DTO.Event;
using EventApi.DTO.Template;
using FluentValidation;

namespace EventApi.Validators.Event
{
    public class CreateEventDtoValidator : AbstractValidator<CreateEventRequestDto>
    {
        public CreateEventDtoValidator()
        {
          
            // Rule 1: Validate the EventInfo JSON string
            RuleFor(x => x.EventInfo)
                .NotEmpty().WithMessage("Event information is required.")
                .Custom((eventInfoJson, context) => {
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

            // Rule 2: Validate the TemplateInfo JSON string (for Pro version)
            RuleFor(x => x.TemplateInfo)
                .NotEmpty().WithMessage("Template information is required.")
                .Custom((templateInfoJson, context) => {
            if (string.IsNullOrEmpty(templateInfoJson)) return;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var templates = JsonSerializer.Deserialize<List<TemplateElementsDto>>(templateInfoJson, options);
                
                if (templates == null || !templates.Any())
                {
                    context.AddFailure("TemplateInfo", "Template elements are required.");
                }
                else if (templates.Any(t => string.IsNullOrWhiteSpace(t.ElementType)))
                {
                    context.AddFailure("TemplateInfo.ElementType", "One or more template elements has a missing or invalid type.");
                }
            }
            catch (JsonException)
            {
                context.AddFailure("TemplateInfo", "The provided template information is not a valid JSON string.");
            }
            });

            // Rule 3: Validate the AttendeeFile
            RuleFor(x => x.AttendeeFile)
                .NotNull().WithMessage("Attendee file is required.");
            
            RuleFor(x => x.AttendeeFile.ContentType)
                .Must(ct => ct == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" || ct == "text/csv")
                .When(x => x.AttendeeFile != null) 
                .WithMessage("Invalid attendee file type. Please upload an XLSX or CSV file.");

            // Rule 4: Validate the BackgroundImage (for Pro version)
            RuleFor(x => x.BackgroundImage)
                .NotNull().WithMessage("Background image is required.");
            
            RuleFor(x => x.BackgroundImage.ContentType)
                .Must(ct => ct == "image/jpeg" || ct == "image/png")
                .When(x => x.BackgroundImage != null) 
                .WithMessage("Invalid background image file type. Please upload a JPG or PNG.");
        }
    
    }
}