namespace GymWorkoutLogApi.Models
{
    public class BodyPart
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<Exercise> Exercises { get; set; } = new();
    }
}
