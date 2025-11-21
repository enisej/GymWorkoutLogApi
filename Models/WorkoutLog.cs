namespace GymWorkoutLogApi.Models
{
    public class WorkoutLog
    {
        public int Id { get; set; }
        public int WorkoutSessionId { get; set; }
        public WorkoutSession WorkoutSession { get; set; } = null!;
        public int ExerciseId { get; set; }
        public Exercise Exercise { get; set; } = null!;
        public int SetNumber { get; set; }
        public decimal WeightKg { get; set; }
        public int Reps { get; set; }
        public string? Notes { get; set; }
    }
}
