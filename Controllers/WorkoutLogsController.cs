using GymWorkoutLogApi.Data;
using GymWorkoutLogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymWorkoutLogApi.Controllers
{
    [ApiController]
    [Route("api/sessions/{sessionId}/logs")]
    public class WorkoutLogsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public WorkoutLogsController(AppDbContext db) => _db = db;

        // POST /api/sessions/5/logs – pievienot setu
        [HttpPost]
        public async Task<ActionResult> Add(int sessionId, [FromBody] CreateLogDto dto)
        {
            var session = await _db.WorkoutSessions.FindAsync(sessionId);
            if (session == null) return NotFound("Session not found");

            var exercise = await _db.Exercises.FindAsync(dto.ExerciseId);
            if (exercise == null) return NotFound("Exercise not found");

            var log = new WorkoutLog
            {
                WorkoutSessionId = sessionId,
                ExerciseId = dto.ExerciseId,  // svarīgi – tikai ID, nevis objekts!
                SetNumber = dto.SetNumber,
                WeightKg = dto.WeightKg,
                Reps = dto.Reps,
                Notes = dto.Notes
            };

            _db.WorkoutLogs.Add(log);
            await _db.SaveChangesAsync();

            return Created("", new { log.Id, log.ExerciseId, log.SetNumber, log.WeightKg, log.Reps });
        }
    }

    public record CreateLogDto(int ExerciseId, int SetNumber, decimal WeightKg, int Reps, string? Notes = null);
}
