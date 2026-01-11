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

        // GET /api/sessions/5/logs
        [HttpGet]
        public async Task<ActionResult> GetSessionLogs(int sessionId)
        {
            var session = await _db.WorkoutSessions.FindAsync(sessionId);
            if (session == null)
                return NotFound("Session not found");

            var logs = await _db.WorkoutLogs
                .Include(l => l.Exercise)
                .Where(l => l.WorkoutSessionId == sessionId)
                .OrderBy(l => l.ExerciseId)
                .ThenBy(l => l.SetNumber)
                .Select(l => new
                {
                    l.Id,
                    ExerciseId = l.ExerciseId,
                    ExerciseName = l.Exercise!.Name,
                    l.SetNumber,
                    WeightKg = l.WeightKg,
                    Reps = l.Reps,
                    l.Notes
                })
                .ToListAsync();

            return Ok(logs);
        }

        // POST /api/sessions/5/logs
        [HttpPost]
        public async Task<ActionResult> Add(int sessionId, [FromBody] CreateLogDto dto)
        {
            var session = await _db.WorkoutSessions.FindAsync(sessionId);
            if (session == null)
                return NotFound("Session not found");

            var exercise = await _db.Exercises.FindAsync(dto.ExerciseId);
            if (exercise == null)
                return NotFound("Exercise not found");

            var log = new WorkoutLog
            {
                WorkoutSessionId = sessionId,
                ExerciseId = dto.ExerciseId,
                SetNumber = dto.SetNumber,
                WeightKg = dto.WeightKg,
                Reps = dto.Reps,
                Notes = dto.Notes
            };

            _db.WorkoutLogs.Add(log);
            await _db.SaveChangesAsync();

            return Created("", new
            {
                log.Id,
                log.ExerciseId,
                log.SetNumber,
                log.WeightKg,
                log.Reps
            });
        }

        // DELETE /api/sessions/5/logs/10
        [HttpDelete("{logId}")]
        public async Task<IActionResult> DeleteLog(int sessionId, int logId)
        {
            var log = await _db.WorkoutLogs
                .FirstOrDefaultAsync(l => l.Id == logId && l.WorkoutSessionId == sessionId);

            if (log == null)
            {
                return NotFound("Log not found or doesn't belong to this session");
            }

            _db.WorkoutLogs.Remove(log);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /api/sessions/5/logs
        [HttpDelete]
        public async Task<IActionResult> DeleteAllSessionLogs(int sessionId)
        {
            var session = await _db.WorkoutSessions.FindAsync(sessionId);
            if (session == null)
                return NotFound("Session not found");

            var logs = await _db.WorkoutLogs
                .Where(l => l.WorkoutSessionId == sessionId)
                .ToListAsync();

            if (logs.Any())
            {
                _db.WorkoutLogs.RemoveRange(logs);
                await _db.SaveChangesAsync();
            }

            return NoContent();
        }
    }

    public record CreateLogDto(int ExerciseId, int SetNumber, decimal WeightKg, int Reps, string? Notes = null);
}