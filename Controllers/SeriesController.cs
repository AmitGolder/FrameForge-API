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
    public class SeriesController : ControllerBase
    {
        private readonly FrameForgeDbContext _context;

        public SeriesController(FrameForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/Series
        [HttpGet]
        public async Task<IActionResult> GetSeries()
        {
            var series = await _context.Series
                .Include(s => s.Brand)
                .OrderBy(s => s.Brand.Name)
                .ThenBy(s => s.Name)
                .Select(s => new
                {
                    s.SeriesId,
                    s.Name,
                    s.BrandId,
                    BrandName = s.Brand.Name
                })
                .ToListAsync();

            return Ok(series);
        }


        // GET: api/Series/brand/5
        [HttpGet("brand/{brandId}")]
        public async Task<IActionResult> GetSeriesByBrand(int brandId)
        {
            var series = await _context.Series
                .Where(s => s.BrandId == brandId)
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    s.SeriesId,
                    s.Name,
                    s.BrandId
                })
                .ToListAsync();

            return Ok(series);
        }

        // GET: api/Series/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSeries(int id)
        {
            var series = await _context.Series
                .Include(s => s.Brand)
                .Where(s => s.SeriesId == id)
                .Select(s => new
                {
                    s.SeriesId,
                    s.Name,
                    s.BrandId,
                    BrandName = s.Brand.Name
                })
                .FirstOrDefaultAsync();

            if (series == null)
            {
                return NotFound();
            }

            return Ok(series);
        }

        // POST: api/Series
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PostSeries(
            [FromBody] Series series)
        {
            if (string.IsNullOrWhiteSpace(series.Name))
            {
                return BadRequest(
                    "Series name is required."
                );
            }

            var brandExists = await _context.Brands
                .AnyAsync(b =>
                    b.BrandId == series.BrandId);

            if (!brandExists)
            {
                return BadRequest(
                    "Selected brand does not exist."
                );
            }

            var name = series.Name.Trim();

            var exists = await _context.Series
                .AnyAsync(s =>
                    s.BrandId == series.BrandId &&
                    s.Name.ToLower() ==
                    name.ToLower());

            if (exists)
            {
                return BadRequest(
                    "This series already exists for the selected brand."
                );
            }

            series.Name = name;

            _context.Series.Add(series);

            await _context.SaveChangesAsync();

            return Ok(series);
        }

        // PUT: api/Series/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSeries(
            int id,
            [FromBody] Series series)
        {
            if (id != series.SeriesId)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(series.Name))
            {
                return BadRequest(
                    "Series name is required."
                );
            }

            var existingSeries = await _context.Series
                .FindAsync(id);

            if (existingSeries == null)
            {
                return NotFound();
            }

            var brandExists = await _context.Brands
                .AnyAsync(b =>
                    b.BrandId == series.BrandId);

            if (!brandExists)
            {
                return BadRequest(
                    "Selected brand does not exist."
                );
            }

            var name = series.Name.Trim();

            var duplicateExists = await _context.Series
                .AnyAsync(s =>
                    s.SeriesId != id &&
                    s.BrandId == series.BrandId &&
                    s.Name.ToLower() ==
                    name.ToLower());

            if (duplicateExists)
            {
                return BadRequest(
                    "This series already exists for the selected brand."
                );
            }

            existingSeries.Name = name;
            existingSeries.BrandId = series.BrandId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Series/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSeries(int id)
        {
            var series = await _context.Series
                .FindAsync(id);

            if (series == null)
            {
                return NotFound();
            }

            var isUsed = await _context.Products
                .AnyAsync(p => p.SeriesId == id);

            if (isUsed)
            {
                return BadRequest(
                    "This series cannot be deleted because it is being used by products."
                );
            }

            _context.Series.Remove(series);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}