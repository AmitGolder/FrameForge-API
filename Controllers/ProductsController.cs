using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FrameForge.Data;
using FrameForge.DTOs;
using FrameForge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrameForge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly FrameForgeDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(
            FrameForgeDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
            [FromQuery] string search = null,
            [FromQuery] int? brandId = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] int? scaleId = null,
            [FromQuery] int? seriesId = null,
            [FromQuery] bool? inStockOnly = null)
        {
            var query = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Brand)
                .Include(p => p.Scale)
                .Include(p => p.Category)
                .Include(p => p.Series)
                .AsQueryable();


            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search)
                );
            }


            // Brand Filter
            if (brandId.HasValue)
            {
                query = query.Where(
                    p => p.BrandId == brandId.Value
                );
            }


            // Category Filter
            if (categoryId.HasValue)
            {
                query = query.Where(
                    p => p.CategoryId == categoryId.Value
                );
            }


            // Scale Filter
            if (scaleId.HasValue)
            {
                query = query.Where(
                    p => p.ScaleId == scaleId.Value
                );
            }


            // Series Filter
            if (seriesId.HasValue)
            {
                query = query.Where(
                    p => p.SeriesId == seriesId.Value
                );
            }


            // In Stock Filter
            if (inStockOnly == true)
            {
                query = query.Where(
                    p => p.IsAvailable &&
                         p.StockQuantity > 0
                );
            }


            var products = await query
                .OrderByDescending(p => p.ProductId)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    IsAvailable = p.IsAvailable,


                    // Brand
                    BrandId = p.BrandId,
                    BrandName = p.Brand != null
                        ? p.Brand.Name
                        : null,


                    // Scale
                    ScaleId = p.ScaleId,
                    ScaleName = p.Scale != null
                        ? p.Scale.Name
                        : null,


                    // Category
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null
                        ? p.Category.Name
                        : null,


                    // Series
                    SeriesId = p.SeriesId,
                    SeriesName = p.Series != null
                        ? p.Series.Name
                        : null,


                    // Images
                    Images = p.ProductImages
                        .Select(pi => pi.ImageUrl)
                        .ToList()
                })
                .ToListAsync();

            return Ok(products);
        }


        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDetailDto>> GetProduct(
            int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Brand)
                .Include(p => p.Scale)
                .Include(p => p.Category)
                .Include(p => p.Series)
                .FirstOrDefaultAsync(
                    p => p.ProductId == id
                );

            if (product == null)
            {
                return NotFound();
            }


            var dto = new ProductDetailDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsAvailable = product.IsAvailable,


                // Brand
                BrandId = product.BrandId,
                BrandName = product.Brand != null
                    ? product.Brand.Name
                    : null,


                // Scale
                ScaleId = product.ScaleId,
                ScaleName = product.Scale != null
                    ? product.Scale.Name
                    : null,


                // Category
                CategoryId = product.CategoryId,
                CategoryName = product.Category != null
                    ? product.Category.Name
                    : null,


                // Series
                SeriesId = product.SeriesId,
                SeriesName = product.Series != null
                    ? product.Series.Name
                    : null,


                // Images
                Images = product.ProductImages
                    .Select(i => i.ImageUrl)
                    .ToList()
            };

            return Ok(dto);
        }


        // PUT: api/Products/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(
            int id,
            [FromBody] Product updatedProduct)
        {
            if (id != updatedProduct.ProductId)
            {
                return BadRequest(
                    "Product ID does not match."
                );
            }

            if (updatedProduct.StockQuantity < 0)
            {
                return BadRequest(
                    "Stock quantity cannot be negative."
                );
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(
                    p => p.ProductId == id
                );

            if (product == null)
            {
                return NotFound();
            }

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;

            product.StockQuantity =
                updatedProduct.StockQuantity;

            // Stock quantity controls availability
            product.IsAvailable =
                product.StockQuantity > 0;

            product.BrandId = updatedProduct.BrandId;
            product.ScaleId = updatedProduct.ScaleId;
            product.CategoryId = updatedProduct.CategoryId;
            product.SeriesId = updatedProduct.SeriesId;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        // POST: api/Products
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(
            [FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (product.StockQuantity < 0)
            {
                return BadRequest(
                    "Stock quantity cannot be negative."
                );
            }

            // Stock quantity controls availability
            product.IsAvailable =
                product.StockQuantity > 0;

            product.CreatedAt = DateTime.Now;

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.ProductId },
                product
            );
        }


        // DELETE: api/Products/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(
            int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(
                    p => p.ProductId == id
                );


            if (product == null)
            {
                return NotFound();
            }


            var imageFolderPath = Path.Combine(
                _environment.WebRootPath,
                "images",
                "products"
            );


            foreach (var image in product.ProductImages)
            {
                var imagePath = Path.Combine(
                    imageFolderPath,
                    image.ImageUrl
                );


                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }


            _context.ProductImages.RemoveRange(
                product.ProductImages
            );


            _context.Products.Remove(product);


            await _context.SaveChangesAsync();


            return NoContent();
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(
                e => e.ProductId == id
            );
        }
    }
}