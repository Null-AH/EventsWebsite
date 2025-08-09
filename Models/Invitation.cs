using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.Models
{
   
    [Table("Invitations")]
    public class Invitation
    {
        public int Id { get; set; }
        public Guid UniqueQRCode { get; set; }

        public int AttendeeId { get; set; } 
        public Attendee Attendee { get; set; }
    }
    
}