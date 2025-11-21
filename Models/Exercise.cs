namespace GymWorkoutLogApi.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<WorkoutLog> WorkoutLogs { get; set; } = new();
        public List<BodyPart> BodyParts { get; set; } = new();
    }
}
