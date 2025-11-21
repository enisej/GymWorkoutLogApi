using GymWorkoutLogApi.Data;
using GymWorkoutLogApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymWorkoutLogApi.Controllers
{
    [ApiController]
    [Route("api/exercises")]
    public class ExercisesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ExercisesController(AppDbContext db) => _db = db;

        // GET /api/exercises – visi vingrinājumi ar bodyPart ID un nosaukumiem
        [HttpGet]
        public async Task<ActionResult> GetAll()
            => Ok(await _db.Exercises
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    BodyPartIds = e.BodyParts.Select(bp => bp.Id).ToArray(),
                    BodyPartNames = e.BodyParts.Select(bp => bp.Name).ToArray()
                })
                .OrderBy(e => e.Name)
                .ToListAsync());

        // GET /api/exercises/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            var ex = await _db.Exercises
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    BodyPartIds = e.BodyParts.Select(bp => bp.Id).ToArray(),
                    BodyPartNames = e.BodyParts.Select(bp => bp.Name).ToArray()
                })
                .FirstOrDefaultAsync();

            return ex == null ? NotFound() : Ok(ex);
        }

        // POST /api/exercises – izveidot jaunu
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateExerciseDto dto)
        {
            var exercise = new Exercise { Name = dto.Name };

            if (dto.BodyPartIds?.Length > 0)
            {
                var bodyParts = await _db.BodyParts
                    .Where(bp => dto.BodyPartIds.Contains(bp.Id))
                    .ToListAsync();
                exercise.BodyParts = bodyParts;
            }

            _db.Exercises.Add(exercise);
            await _db.SaveChangesAsync();

            // Atgriežam tikai ID un vārdu – bez cikliem!
            return CreatedAtAction(nameof(Get), new { id = exercise.Id }, new { exercise.Id, exercise.Name });
        }

        // PUT /api/exercises/5 – update
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] CreateExerciseDto dto)
        {
            var exercise = await _db.Exercises.Include(e => e.BodyParts).FirstOrDefaultAsync(e => e.Id == id);
            if (exercise == null) return NotFound();

            exercise.Name = dto.Name;
            exercise.BodyParts.Clear();

            if (dto.BodyPartIds != null && dto.BodyPartIds.Length > 0)
            {
                exercise.BodyParts.AddRange(await _db.BodyParts
                    .Where(bp => dto.BodyPartIds.Contains(bp.Id))
                    .ToListAsync());
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/exercises/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var exercise = await _db.Exercises.FindAsync(id);
            if (exercise == null) return NotFound();

            _db.Exercises.Remove(exercise);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    public record CreateExerciseDto(string Name, int[]? BodyPartIds = null);
}
