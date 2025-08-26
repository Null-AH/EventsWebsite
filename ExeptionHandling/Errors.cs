using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EventApi.ExeptionHandling
{

    public static class UserErrors
    {
        public static readonly Error SubscriptionLimit = new(
            "User.SubscriptionLimit",
            "You cannot apply this action within your current subscription plan."
        );
        public static readonly Error CreationFailed = new(
            "User.CreationFailed",
            ""
        );
        public static readonly Error UpdateFailed = new(
            "User.UpdateFailed",
            ""
        );
    }
    public static class EventErrors
    {
        public static readonly Error EventsNotFound = new(
            "Event.NotFound",
            "You do not have any events yet"
        );
        public static readonly Error EventIdNotFound = new(
            "Event.NotFound",
            "The event with the specific ID was not found."
        );
        public static readonly Error LimitReached = new(
            "Event.LimitReached",
            "You have reached the maximum number of events you can create within your current subscription plan."
        );
    }
    public static class AttendeeErrors
    {
        public static readonly Error NotFound = new(
            "Attendee.NotFound",
            "The attendee with the provided email could not be found for this event.");

        public static readonly Error AlreadyCheckedIn = new(
            "Attendee.AlreadyCheckedIn",
            "This attendee has already been checked in.");
    }

    public static class CollaboratorErrors
    {
        public static readonly Error CollaboratorsNotFound = new(
            "Collaborator.NotFound",
            "Collaborator not found"
        );
        public static readonly Error CollaboratorCreatorEditDelete = new(
            "Collaborator.Validation",
            "Only the event creator can demote another owner."
        );
        public static readonly Error CollaboratorLastOwnerEditDelete = new(
            "Collaborator.Validation",
            "Cannot remove or downgrade the last owner of an event. Please assign a new owner first."
        );
        public static readonly Error DeleteAllOwners = new(
            "Collaborator.Validation",
            "You cannot remove all owners of an event. Please leave at least one."
        );
        public static readonly Error NonCreatorOwnerDelete = new(
            "Collaborator.Validation",
            "Only the event creator can remove another owner while they are still a collaborator."
            );
        public static readonly Error EditorNotPermitedAction = new(
            "Collaborator.Forbidden",
            "As an editor You do not have permission to perform this action"
            );
        public static readonly Error CheckInStaffNotPermitedAction = new(
            "Collaborator.Forbidden",
            "As a Check-In staff You do not have permission to perform this action"
            );
        public static readonly Error GeneralNotPermitedAction = new(
            "Collaborator.Forbidden",
            "Your role does not grant You permission to perform the action"
            );       

    }

    public static class FileErrors
    {
        public static readonly Error InvalidHeaders = new(
            "File.Validation",
            "The uploaded file is missing one or more required column headers (e.g., 'Name', 'Email').");

        public static readonly Error NoAttendeeFound = new(
            "File.Validation",
            "The uploaded file does not contain any valid attendees with both a name and an email.");
        public static readonly Error OneOrMoreAttendeeNotValid = new(
            "File.Validation",
            "One or mote attendees name or email is unvalid or missing.");
        public static readonly Error UnsupportedType = new(
            "File.Validation",
            "Invalid attendee file type. Please upload an XLSX or CSV file."
        );
        public static readonly Error NullOrEmpty = new(
            "File.Validation",
            "The File is either corupted or not found.");

        public static readonly Error SaveFailed = new(
            "File.NotFound",
            "The file was not saved sucessfully please try again."
        );
    }

}