using GymWorkoutLogApi.Models;

namespace GymWorkoutLogApi.Data
{
    public static class SeedData
    {
        public static async Task EnsureCreated(AppDbContext db)
        {
            // Ja jau ir dati – neko nedarām
            if (db.Users.Any() || db.Exercises.Any()) return;

            var rnd = new Random();

            // 1. BodyParts
            var bodyPartNames = new[] { "Chest", "Back", "Legs", "Shoulders", "Biceps", "Triceps", "Abs", "Calves" };
            var bodyParts = bodyPartNames.Select(name => new BodyPart { Name = name }).ToList();
            db.BodyParts.AddRange(bodyParts);
            await db.SaveChangesAsync();

            // 2. Exercises + saites uz BodyParts
            var exerciseData = new (string Name, string[] Parts)[]
            {
            ("Bench Press", new[] { "Chest", "Triceps" }),
            ("Incline Bench Press", new[] { "Chest", "Shoulders" }),
            ("Squat", new[] { "Legs", "Abs" }),
            ("Deadlift", new[] { "Back", "Legs" }),
            ("Pull-Up", new[] { "Back", "Biceps" }),
            ("Barbell Row", new[] { "Back", "Biceps" }),
            ("Overhead Press", new[] { "Shoulders", "Triceps" }),
            ("Lunges", new[] { "Legs" }),
            ("Bicep Curl", new[] { "Biceps" }),
            ("Tricep Extension", new[] { "Triceps" }),
            ("Plank", new[] { "Abs" })
            };

            foreach (var (name, parts) in exerciseData)
            {
                var ex = new Exercise { Name = name };
                ex.BodyParts.AddRange(bodyParts.Where(bp => parts.Contains(bp.Name)));
                db.Exercises.Add(ex);
            }
            await db.SaveChangesAsync();

            // 3. User
            var user = new User { Name = "Ilja" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            // 4. 25 treniņu sesijas pēdējo 90 dienu laikā
            var allExercises = db.Exercises.ToList();

            for (int day = 0; day < 25; day++)
            {
                var sessionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-rnd.Next(0, 90)));

                var session = new WorkoutSession
                {
                    User = user,
                    Date = sessionDate,
                    Name = rnd.Next(0, 3) switch
                    {
                        0 => "Push Day",
                        1 => "Pull Day",
                        _ => "Leg Day"
                    },
                    Notes = rnd.Next(0, 4) == 0 ? "Feeling strong today!" : null
                };

                // Katrai sesijai 4–7 random vingrinājumi
                var exercisesInSession = allExercises.OrderBy(x => rnd.Next()).Take(rnd.Next(4, 8)).ToList();

                foreach (var ex in exercisesInSession)
                {
                    int sets = rnd.Next(3, 6); // 3–5 seti
                    for (int s = 1; s <= sets; s++)
                    {
                        session.Logs.Add(new WorkoutLog
                        {
                            Exercise = ex,
                            SetNumber = s,
                            WeightKg = (decimal)Math.Round(rnd.NextDouble() * 160 + 20, 1),
                            Reps = rnd.Next(5, 16), // 5–15 reps
                            Notes = rnd.Next(0, 10) == 0 ? "PR!" : null
                        });
                    }
                }

                db.WorkoutSessions.Add(session);
            }

            await db.SaveChangesAsync();
        }
    }
}
