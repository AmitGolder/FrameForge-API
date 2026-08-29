using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameForge.Data;
using FrameForge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrameForge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScalesController : ControllerBase
    {
        private readonly FrameForgeDbContext _context;

        public ScalesController(FrameForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/Scales
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Scale>>> GetScales()
        {
            return await _context.Scales
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        // GET: api/Scales/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Scale>> GetScale(int id)
        {
            var scale = await _context.Scales.FindAsync(id);

            if (scale == null)
            {
                return NotFound();
            }

            return scale;
        }

        // POST: api/Scales
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Scale>> PostScale(
            [FromBody] Scale scale)
        {
            if (string.IsNullOrWhiteSpace(scale.Name))
            {
                return BadRequest("Scale name is required.");
            }

            var name = scale.Name.Trim();

            var exists = await _context.Scales
                .AnyAsync(s =>
                    s.Name.ToLower() == name.ToLower());

            if (exists)
            {
                return BadRequest(
                    "A scale with this name already exists."
                );
            }

            scale.Name = name;

            _context.Scales.Add(scale);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetScale),
                new { id = scale.ScaleId },
                scale
            );
        }

        // PUT: api/Scales/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutScale(
            int id,
            [FromBody] Scale scale)
        {
            if (id != scale.ScaleId)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(scale.Name))
            {
                return BadRequest("Scale name is required.");
            }

            var name = scale.Name.Trim();

            var exists = await _context.Scales
                .AnyAsync(s =>
                    s.ScaleId != id &&
                    s.Name.ToLower() == name.ToLower());

            if (exists)
            {
                return BadRequest(
                    "A scale with this name already exists."
                );
            }

            var existingScale = await _context.Scales
                .FindAsync(id);

            if (existingScale == null)
            {
                return NotFound();
            }

            existingScale.Name = name;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Scales/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScale(int id)
        {
            var scale = await _context.Scales.FindAsync(id);

            if (scale == null)
            {
                return NotFound();
            }

            var isUsed = await _context.Products
                .AnyAsync(p => p.ScaleId == id);

            if (isUsed)
            {
                return BadRequest(
                    "This scale cannot be deleted because it is being used by products."
                );
            }

            _context.Scales.Remove(scale);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}