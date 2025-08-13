using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EventApi.Services;
using NPOI.SS.Formula.Functions;

namespace EventApi.Models
{

[JsonConverter(typeof(JsonStringEnumMemberConverter<Role>))]
    public enum Role
    {
        Owner,
        Editor,
        [EnumMember(Value = "Check-In Staff")]
        CheckInStaff
    }
    

    [Table("EventCollaborators")]
    public class EventCollaborators
    {

        public string UserId { get; set; }
        public AppUser AppUser { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public Role Role { get; set; }
    }
}