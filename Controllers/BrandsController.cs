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
    public class BrandsController : ControllerBase
    {
        private readonly FrameForgeDbContext _context;

        public BrandsController(FrameForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/Brands
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrands()
        {
            return await _context.Brands
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        // GET: api/Brands/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Brand>> GetBrand(int id)
        {
            var brand = await _context.Brands
                .FindAsync(id);

            if (brand == null)
            {
                return NotFound();
            }

            return brand;
        }

        // POST: api/Brands
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Brand>> PostBrand(
            [FromBody] Brand brand)
        {
            if (string.IsNullOrWhiteSpace(brand.Name))
            {
                return BadRequest("Brand name is required.");
            }

            var exists = await _context.Brands
                .AnyAsync(b =>
                    b.Name.ToLower() ==
                    brand.Name.ToLower());

            if (exists)
            {
                return BadRequest(
                    "A brand with this name already exists."
                );
            }

            brand.Name = brand.Name.Trim();

            _context.Brands.Add(brand);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetBrand),
                new { id = brand.BrandId },
                brand
            );
        }

        // PUT: api/Brands/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBrand(
            int id,
            [FromBody] Brand brand)
        {
            if (id != brand.BrandId)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(brand.Name))
            {
                return BadRequest(
                    "Brand name is required."
                );
            }

            var exists = await _context.Brands
                .AnyAsync(b =>
                    b.BrandId != id &&
                    b.Name.ToLower() ==
                    brand.Name.ToLower());

            if (exists)
            {
                return BadRequest(
                    "A brand with this name already exists."
                );
            }

            var existingBrand = await _context.Brands
                .FindAsync(id);

            if (existingBrand == null)
            {
                return NotFound();
            }

            existingBrand.Name = brand.Name.Trim();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Brands/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var brand = await _context.Brands
                .FindAsync(id);

            if (brand == null)
            {
                return NotFound();
            }

            _context.Brands.Remove(brand);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}