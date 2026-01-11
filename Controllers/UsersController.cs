using GymWorkoutLogApi.Data;
using GymWorkoutLogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymWorkoutLogApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) => _db = db;

        // GET /api/users
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var users = await _db.Users
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    SessionCount = u.Sessions.Count
                })
                .OrderBy(u => u.Name)
                .ToListAsync();

            return Ok(users);
        }

        // GET /api/users/5 
        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            var user = await _db.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    SessionCount = u.Sessions.Count
                })
                .FirstOrDefaultAsync();

            return user == null ? NotFound() : Ok(user);
        }

        // GET /api/users/name/{name}
        [HttpGet("name/{name}")]
        public async Task<ActionResult> GetByName(string name)
        {
            var user = await _db.Users
                .Where(u => u.Name == name)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    SessionCount = u.Sessions.Count
                })
                .FirstOrDefaultAsync();

            return user == null ? NotFound() : Ok(user);
        }

        // POST /api/users
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateUserDto dto)
        {
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Name == dto.Name);
            if (existing != null)
            {
                return BadRequest("User with this name already exists");
            }

            var user = new User { Name = dto.Name };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new
            {
                id = user.Id
            }, new
            {
                user.Id,
                user.Name
            });
        }

        // PUT /api/users/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] CreateUserDto dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Name == dto.Name && u.Id != id);
            if (existing != null)
            {
                return BadRequest("User with this name already exists");
            }

            user.Name = dto.Name;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /api/users/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }

    public record CreateUserDto(string Name);
}