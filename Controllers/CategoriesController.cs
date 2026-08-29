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
    public class CategoriesController : ControllerBase
    {
        private readonly FrameForgeDbContext _context;

        public CategoriesController(FrameForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // POST: api/Categories
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(
            [FromBody] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return BadRequest("Category name is required.");
            }

            var name = category.Name.Trim();

            var exists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == name.ToLower());

            if (exists)
            {
                return BadRequest(
                    "A category with this name already exists."
                );
            }

            category.Name = name;

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryId },
                category
            );
        }

        // PUT: api/Categories/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(
            int id,
            [FromBody] Category category)
        {
            if (id != category.CategoryId)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return BadRequest(
                    "Category name is required."
                );
            }

            var name = category.Name.Trim();

            var exists = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryId != id &&
                    c.Name.ToLower() == name.ToLower()
                );

            if (exists)
            {
                return BadRequest(
                    "A category with this name already exists."
                );
            }

            var existingCategory = await _context.Categories
                .FindAsync(id);

            if (existingCategory == null)
            {
                return NotFound();
            }

            existingCategory.Name = name;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Categories/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var isUsed = await _context.Products
                .AnyAsync(p => p.CategoryId == id);

            if (isUsed)
            {
                return BadRequest(
                    "This category cannot be deleted because it is being used by products."
                );
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}