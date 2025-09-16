using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace EventApi.Models
{
    public enum SubscriptionTier {
        Free,
        Pro
    }
    public class AppUser : IdentityUser
    {
        public string? FirebaseUid { get; set; }
        public string? PictureUrl { get; set; }
        public string? DisplayName { get; set; }
        public SubscriptionTier? Tier { get; set; }
        
        public List<EventCollaborators> EventCollaborators { get; set; } = new List<EventCollaborators>();

    }
}