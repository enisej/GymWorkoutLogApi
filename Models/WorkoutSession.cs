namespace GymWorkoutLogApi.Models
{
    public class WorkoutSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public string? Name { get; set; }
        public string? Notes { get; set; }
        public List<WorkoutLog> Logs { get; set; } = new();
    }
}
