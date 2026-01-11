using System.Text.Json;
using GymWorkoutLogApi.Data;
using GymWorkoutLogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymWorkoutLogApi.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public class WorkoutSessionsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public WorkoutSessionsController(AppDbContext db) => _db = db;

        [HttpPost("query")]
        public async Task<IActionResult> Query([FromBody] JsonElement filter)
        {
            if (!filter.TryGetProperty("exerciseName", out var exProp) ||
                !filter.TryGetProperty("fromDate", out var fromProp) ||
                !filter.TryGetProperty("toDate", out var toProp))
            {
                return BadRequest("exerciseName, fromDate and toDate are required.");
            }

            var exerciseName = exProp.GetString();
            var fromDateStr = fromProp.GetString();
            var toDateStr = toProp.GetString();

            if (string.IsNullOrEmpty(exerciseName) ||
                string.IsNullOrEmpty(fromDateStr) ||
                string.IsNullOrEmpty(toDateStr))
            {
                return BadRequest("exerciseName, fromDate and toDate are required.");
            }

            if (!DateOnly.TryParse(fromDateStr, out var fromDate) ||
                !DateOnly.TryParse(toDateStr, out var toDate))
            {
                return BadRequest("Invalid date format.");
            }

            var logs = await _db.WorkoutLogs
                .Include(l => l.Exercise)
                .Include(l => l.WorkoutSession)
                .Where(l => l.Exercise!.Name == exerciseName
                            && l.WorkoutSession.Date >= fromDate
                            && l.WorkoutSession.Date <= toDate)
                .OrderByDescending(l => l.WorkoutSession.Date)
                .Select(l => new
                {
                    Exercise = l.Exercise!.Name,
                    l.WeightKg,
                    l.Reps,
                    VolumeKg = l.WeightKg * l.Reps,
                    Date = l.WorkoutSession.Date,
                    l.Notes
                })
                .ToListAsync();

            return Ok(logs);
        }

        // GET /api/sessions – visi treniņi
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var sessions = await _db.WorkoutSessions
                .Include(s => s.Logs!)
                    .ThenInclude(l => l.Exercise)
                .OrderByDescending(s => s.Date)
                .Select(s => new
                {
                    s.Id,
                    s.Date,
                    s.Name,
                    s.Notes,
                    Logs = s.Logs!.Select(l => new
                    {
                        l.Id,
                        l.SetNumber,
                        ExerciseId = l.ExerciseId,  // ← ADDED THIS
                        ExerciseName = l.Exercise!.Name,
                        WeightKg = l.WeightKg,
                        Reps = l.Reps,
                        l.Notes
                    })
                })
                .ToListAsync();

            return Ok(sessions);
        }

        // GET /api/sessions/5 – pilns treniņš ar setiem
        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            var session = await _db.WorkoutSessions
                .Include(s => s.Logs!)
                    .ThenInclude(l => l.Exercise)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Date,
                    s.Name,
                    s.Notes,
                    Logs = s.Logs!.Select(l => new
                    {
                        l.Id,
                        l.SetNumber,
                        ExerciseId = l.ExerciseId,  // ← ADDED THIS
                        ExerciseName = l.Exercise!.Name,
                        WeightKg = l.WeightKg,
                        Reps = l.Reps,
                        l.Notes
                    })
                })
                .FirstOrDefaultAsync();

            return session == null ? NotFound() : Ok(session);
        }

        // POST /api/sessions – jauns treniņš
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateSessionDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == "Ilja");
            if (user == null)
            {
                user = new User { Name = "Ilja" };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }

            var session = new WorkoutSession
            {
                UserId = user.Id,
                Date = dto.Date,
                Name = dto.Name,
                Notes = dto.Notes
            };

            _db.WorkoutSessions.Add(session);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new
            {
                id = session.Id
            }, new
            {
                session.Id,
                session.Date,
                session.Name
            });
        }

        // PUT /api/sessions/5 – update
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] CreateSessionDto dto)
        {
            var session = await _db.WorkoutSessions.FindAsync(id);
            if (session == null)
                return NotFound();

            session.Date = dto.Date;
            session.Name = dto.Name;
            session.Notes = dto.Notes;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/sessions/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var session = await _db.WorkoutSessions.FindAsync(id);
            if (session == null)
                return NotFound();

            _db.WorkoutSessions.Remove(session);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    public record CreateSessionDto(DateOnly Date, string? Name = null, string? Notes = null);
}