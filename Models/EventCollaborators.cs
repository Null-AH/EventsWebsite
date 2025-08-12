using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using NPOI.SS.Formula.Functions;

namespace EventApi.Models
{
    public enum Role
    {
        Owner,
        Editor,
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