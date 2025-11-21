namespace GymWorkoutLogApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<WorkoutSession> Sessions { get; set; } = new();
    }
}
