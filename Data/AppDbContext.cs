using GymWorkoutLogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GymWorkoutLogApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Exercise> Exercises => Set<Exercise>();
        public DbSet<BodyPart> BodyParts => Set<BodyPart>();
        public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
        public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Exercise AND BodyPart many-to-many
            modelBuilder.Entity<Exercise>()
                .HasMany(e => e.BodyParts)
                .WithMany(b => b.Exercises)
                .UsingEntity<Dictionary<string, object>>(
                    "ExerciseBodyPart",
                    j => j.HasOne<BodyPart>().WithMany().HasForeignKey("BodyPartId"),
                    j => j.HasOne<Exercise>().WithMany().HasForeignKey("ExerciseId"));
        }
    }
}
