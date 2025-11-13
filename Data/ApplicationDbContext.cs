using New_LeRayBookingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using New_LeRayBookingSystem.ViewModels;
using System.Diagnostics.CodeAnalysis; // Added for Code Analysis Attributes

namespace New_LeRayBookingSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // --- Model DbSets (These map to database tables) ---
        public DbSet<Appointment> Appointments { get; set; } = default!; 
        public DbSet<Service> Services { get; set; } = default!;      
        public DbSet<Feedback> Feedbacks { get; set; } = default!;    
        public DbSet<Promos> Promos { get; set; } = default!;         
        public DbSet<Payments> Payments { get; set; } = default!;      
        public DbSet<AuditLog> AuditLogs { get; set; } = default!;
        public DbSet<CustomerService> CustomerServices { get; set; }  
       public DbSet<AppointmentService> AppointmentServices { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- FIX: Seed the missing Service Bundle placeholder configuration ---
            // This is the record the BookingController is failing to find.
            builder.Entity<CustomerService>().HasData(
                new CustomerService
                {
                    Id = 9999, // Use a high, unique integer ID for the placeholder
                    ServiceName = "Service Bundle Placeholder",
                    Description = "Internal service used to handle aggregated bookings and downpayment logic.",
                    Price = 650.00m, // Matching the base price from the screenshot
                    Duration = new TimeSpan(0, 30, 0),
                    IsBundle = true, // Placeholder duration (30 minutes)
                    Category = "Bundle", // <--- THE CRITICAL FIX for s.Category == "Bundle"
                }
            );
            // --- End of Fix ---

            // Fix for Appointment foreign key (Assuming ApplicationUser.Id is string)
            builder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Standard Identity Entity Configuration (Your existing type mappings for MySQL/MariaDB)

            // IdentityRole
            builder.Entity<IdentityRole>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("varchar(255)");
                entity.Property(e => e.Name).HasMaxLength(256).HasColumnType("varchar(256)");
                entity.Property(e => e.NormalizedName).HasMaxLength(256).HasColumnType("varchar(256)");
                entity.Property(e => e.ConcurrencyStamp).HasColumnType("longtext");
            });

            // ApplicationUser (inherits IdentityUser)
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("varchar(255)");
                entity.Property(e => e.UserName).HasMaxLength(256).HasColumnType("varchar(256)");
                entity.Property(e => e.NormalizedUserName).HasMaxLength(256).HasColumnType("varchar(256)");
                entity.Property(e => e.Email).HasMaxLength(256).HasColumnType("varchar(256)");
                entity.Property(e => e.NormalizedEmail).HasMaxLength(256).HasColumnType("varchar(256)");
                entity.Property(e => e.ConcurrencyStamp).HasColumnType("longtext");
                entity.Property(e => e.SecurityStamp).HasColumnType("longtext");
                entity.Property(e => e.PhoneNumber).HasColumnType("varchar(20)");
            });

            // IdentityUserToken
            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.Property(e => e.Value).HasColumnType("longtext");
            });

            // IdentityUserLogin
            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.Property(e => e.ProviderDisplayName).HasColumnType("longtext");
            });

            // IdentityUserClaim
            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.Property(e => e.ClaimType).HasColumnType("longtext");
                entity.Property(e => e.ClaimValue).HasColumnType("longtext");
            });

            // IdentityRoleClaim
            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.Property(e => e.ClaimType).HasColumnType("longtext");
                entity.Property(e => e.ClaimValue).HasColumnType("longtext");
            });
        }
    }
}