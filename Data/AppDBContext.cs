using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Modes;

namespace EventApi.Data
{
    public class AppDBContext : IdentityDbContext
    {
        public AppDBContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {

        }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<TemplateElement> TemplateElements { get; set; }
        public DbSet<Attendee> Attendees { get; set; }
        public DbSet<CollaboratorsInvitation> CollaboratorsInvitations { get; set; }
        public DbSet<EventCollaborators> EventCollaborators { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // List<IdentityRole> roles = new List<IdentityRole>
            // {
            //     new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
            //     new IdentityRole { Name = "User", NormalizedName = "USER" }
            // };

            // builder.Entity<IdentityRole>().HasData(roles);
            builder.Entity<EventCollaborators>().HasKey(ec => new { ec.UserId, ec.EventId });
            builder.Entity<EventCollaborators>().HasOne(ec => ec.AppUser).WithMany(a => a.EventCollaborators).HasForeignKey(ec => ec.UserId);
            builder.Entity<EventCollaborators>().HasOne(ec => ec.Event).WithMany(e => e.EventCollaborators).HasForeignKey(ec => ec.EventId);

            builder.Entity<AppUser>().ToTable("AspNetUsers");
            builder.Entity<IdentityRole>().ToTable("AspNetRoles");
            builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
            builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
            builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");
        
        }
    }
}