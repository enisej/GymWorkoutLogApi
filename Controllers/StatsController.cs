using GymWorkoutLogApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymWorkoutLogApi.Controllers
{
    [ApiController]
    [Route("api/stats")]
    public class StatsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public StatsController(AppDbContext db) => _db = db;

        // 1. TOP 10 smagākie seti (visā vēsturē)
        [HttpGet("top10")]
        public async Task<IActionResult> Top10()
        {
            var top = await _db.WorkoutLogs
                .Include(l => l.Exercise)
                .Include(l => l.WorkoutSession)
                .OrderByDescending(l => l.WeightKg * l.Reps) // volume = kg × reps
                .Take(10)
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

            return Ok(top);
        }

        // 2. Visi seti virs noteikta svara (default 100 kg)
        [HttpGet("heavy")]
        public async Task<IActionResult> Heavy([FromQuery] decimal minWeight = 100)
        {
            var heavy = await _db.WorkoutLogs
                .Include(l => l.Exercise)
                .Where(l => l.WeightKg >= minWeight)
                .OrderByDescending(l => l.WeightKg)
                .Take(30)
                .Select(l => new
                {
                    Exercise = l.Exercise!.Name,
                    l.WeightKg,
                    l.Reps,
                    Date = l.WorkoutSession.Date
                })
                .ToListAsync();

            return Ok(heavy);
        }

        // 3. Pilna sesija konkrētā datumā
        [HttpGet("by-date")]
        public async Task<IActionResult> ByDate([FromQuery] DateOnly date)
        {
            var session = await _db.WorkoutSessions
                .Where(s => s.Date == date)
                .Include(s => s.Logs!)
                    .ThenInclude(l => l.Exercise!)
                    .ThenInclude(e => e!.BodyParts)
                .Select(s => new
                {
                    s.Id,
                    s.Date,
                    s.Name,
                    s.Notes,
                    Logs = s.Logs!.Select(l => new
                    {
                        Exercise = l.Exercise!.Name,
                        l.SetNumber,
                        l.WeightKg,
                        l.Reps,
                        l.Notes
                    })
                })
                .FirstOrDefaultAsync();

            return session == null ? NotFound("No workout on this date") : Ok(session);
        }

        // 4. Vingrinājumi pēc muskuļu grupas
        [HttpGet("by-bodypart")]
        public async Task<IActionResult> ByBodyPart([FromQuery] string name)
        {
            var exercises = await _db.Exercises
                .Where(e => e.BodyParts.Any(bp => EF.Functions.ILike(bp.Name, $"%{name}%")))
                .Include(e => e.BodyParts)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    BodyParts = e.BodyParts.Select(bp => bp.Name)
                })
                .ToListAsync();

            return Ok(exercises);
        }

        // 5. Sesiju skaits pēdējās 30 dienās
        [HttpGet("last30days")]
        public async Task<IActionResult> Last30Days()
        {
            var count = await _db.WorkoutSessions
                .CountAsync(s => s.Date >= DateOnly.FromDateTime(DateTime.Today.AddDays(-30)));

            return Ok(new { SessionsLast30Days = count });
        }

        // 6. Populārākās muskuļu grupas (pēc setu skaita)
        [HttpGet("popular-bodyparts")]
        public async Task<IActionResult> PopularBodyParts()
        {
            var stats = await _db.WorkoutLogs
                .Include(l => l.Exercise!)
                    .ThenInclude(e => e!.BodyParts)
                .SelectMany(l => l.Exercise!.BodyParts.Select(bp => bp.Name))
                .GroupBy(name => name)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new { BodyPart = g.Key, SetsCount = g.Count() })
                .ToListAsync();

            return Ok(stats);
        }
    }
}
