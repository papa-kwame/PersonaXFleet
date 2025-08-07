using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonaXFleet.Models;

namespace PersonaXFleet.Data
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<VehicleAssignmentHistory> VehicleAssignmentHistories { get; set; }
        public DbSet<Router> Routes { get; set; }
        public DbSet<UserRouteRole> UserRouteRoles { get; set; }
        
        public DbSet<PartUsed> PartsUsed { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

       public DbSet<MaintenanceTransaction> MaintenanceTransactions { get; set; }
        public DbSet<VehicleRequest> VehicleRequest{ get; set;  }

        public DbSet<MaintenanceApproval> MaintenanceApprovals { get; set; }
        public DbSet<MaintenanceHistory> MaintenanceHistories { get; set; }

        public DbSet<MaintenanceDocument> MaintenanceDocuments { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<MaintenanceComment> MaintenanceComments { get; set; }
        public DbSet<VehicleAssignmentRequest> VehicleAssignmentRequests { get; set; }
        public DbSet<VehicleAssignmentTransaction> VehicleAssignmentTransactions { get; set; }

        public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
        public DbSet<FuelLog> FuelLogs { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<ActivityLog> ActivityLogs { get; set; }

        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<MaintenanceProgressUpdate> MaintenanceProgressUpdates { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Configure enums to be stored as strings
            modelBuilder
                .Entity<Vehicle>()
                .Property(e => e.Status)
                .HasConversion<string>();

            modelBuilder
                .Entity<Vehicle>()
                .Property(e => e.FuelType)
                .HasConversion<string>();

            modelBuilder
                .Entity<Vehicle>()
                .Property(e => e.Transmission)
                .HasConversion<string>();

     
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict) // Or DeleteBehavior.SetNull
                .IsRequired(false);
            modelBuilder.Entity<MaintenanceRequest>()
             .HasMany(r => r.Transactions)
             .WithOne()
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VehicleAssignmentHistory>()
                  .HasOne(vah => vah.Vehicle)
                  .WithMany(v => v.AssignmentHistory)
                  .HasForeignKey(vah => vah.VehicleId)
                  .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VehicleAssignmentHistory>()
                .HasOne(vah => vah.User)
                .WithMany()
                .HasForeignKey(vah => vah.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Router>()
                 .HasMany(r => r.UserRoles)
                 .WithOne(ur => ur.Route)
                 .HasForeignKey(ur => ur.RouteId)
                 .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRouteRole>()
                .HasOne(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaintenanceHistory>()
                .HasKey(h => h.HistoryId);

            modelBuilder.Entity<MaintenanceComment>()
                .HasOne(c => c.MaintenanceRequest)
                .WithMany()
                .HasForeignKey(c => c.MaintenanceRequestId)
                .OnDelete(DeleteBehavior.NoAction); // Changed from Cascade

            // Keep cascade for Author since it's likely the only path
            modelBuilder.Entity<MaintenanceComment>()
                .HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
     
            modelBuilder.Entity<MaintenanceTransaction>()
                .HasOne(mt => mt.MaintenanceRequest)
                .WithMany(mr => mr.Transactions)
                .HasForeignKey(mt => mt.MaintenanceRequestId)
                .OnDelete(DeleteBehavior.NoAction); // or DeleteBehavior.SetNull

            // Prevent cascade delete between VehicleAssignmentTransaction and VehicleAssignmentRequest
            modelBuilder.Entity<VehicleAssignmentTransaction>()
                .HasOne(t => t.Request)
                .WithMany(r => r.Transactions) // Ensure VehicleAssignmentRequest has a `public ICollection<VehicleAssignmentTransaction> Transactions { get; set; }`
                .HasForeignKey(t => t.VehicleAssignmentRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent cascade delete between VehicleAssignmentTransaction and IdentityUser
            modelBuilder.Entity<VehicleAssignmentTransaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FuelLog>()
                .HasOne(f => f.Vehicle)
                .WithMany() // or .WithMany(v => v.FuelLogs) if you add a collection in `Vehicle`
                .HasForeignKey(f => f.VehicleId);


        }
    }
}