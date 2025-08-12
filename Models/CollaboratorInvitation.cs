using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.Models
{
    public enum Status
    {
        Pending,
        Revoked,
        Deleted
    }
    
   
    [Table("CollaboratorsInvitations")]
    public class CollaboratorsInvitation
    {        
        public int Id { get; set; }
        public string InvitedEmail { get; set; }
        public Status Status { get; set; }
        public Role Role { get; set; }
        public string InvitationToken { get; set; }

        public int EventId { get; set; } 
        public Event Event { get; set; }
    }
    
}