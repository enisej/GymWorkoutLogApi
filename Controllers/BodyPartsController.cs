using GymWorkoutLogApi.Data;
using GymWorkoutLogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymWorkoutLogApi.Controllers
{
    [ApiController]
    [Route("api/bodyparts")]
    public class BodyPartsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public BodyPartsController(AppDbContext db) => _db = db;

        // GET /api/bodyparts – visi body parts
        [HttpGet]
        public async Task<ActionResult> GetAll()
            => Ok(await _db.BodyParts
                .Select(bp => new { bp.Id, bp.Name })
                .OrderBy(bp => bp.Name)
                .ToListAsync());

        // GET /api/bodyparts/1/exercises – vingrinājumi konkrētai muskuļu grupai
        [HttpGet("{id:int}/exercises")]
        public async Task<ActionResult> GetExercises(int id)
            => Ok(await _db.Exercises
                .Where(e => e.BodyParts.Any(bp => bp.Id == id))
                .Select(e => new { e.Id, e.Name })
                .ToListAsync());

        // POST /api/bodyparts – izveidot jaunu
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateBodyPartDto dto)
        {
            var bp = new BodyPart { Name = dto.Name };
            _db.BodyParts.Add(bp);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = bp.Id }, new { bp.Id, bp.Name });
        }

        // DELETE /api/bodyparts/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var bp = await _db.BodyParts.FindAsync(id);
            if (bp == null) return NotFound();
            _db.BodyParts.Remove(bp);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    public record CreateBodyPartDto(string Name);
}
