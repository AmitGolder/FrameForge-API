using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FrameForge.Data;
using FrameForge.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace FrameForge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductImagesController : ControllerBase
    {
        private readonly FrameForgeDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductImagesController(
            FrameForgeDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/ProductImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductImage>>> GetProductImages()
        {
            return await _context.ProductImages.ToListAsync();
        }

        // GET: api/ProductImages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductImage>> GetProductImage(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);

            if (productImage == null)
            {
                return NotFound();
            }

            return productImage;
        }

        // PUT: api/ProductImages/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductImage(int id, ProductImage productImage)
        {
            if (id != productImage.ProductImageId)
            {
                return BadRequest();
            }

            _context.Entry(productImage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductImageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ProductImages
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ProductImage>> PostProductImage(ProductImage productImage)
        {
            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "GetProductImage",
                new { id = productImage.ProductImageId },
                productImage);
        }

        // DELETE: api/ProductImages/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductImage(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);

            if (productImage == null)
            {
                return NotFound();
            }

            var imagePath = Path.Combine(
                _environment.WebRootPath,
                "images",
                "products",
                productImage.ImageUrl
            );

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _context.ProductImages.Remove(productImage);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("upload-multiple/{productId}")]
        public async Task<IActionResult> UploadMultipleImages(
            int productId,
            List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var uploadPath = Path.Combine(
                _environment.WebRootPath,
                "images",
                "products"
            );

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var cleanProductName = product.Name
                .Trim()
                .ToLower()
                .Replace(" ", "-");

            int imageCounter = 1;

            foreach (var file in files)
            {
                if (file.Length <= 0)
                {
                    continue;
                }

                var extension = Path
                    .GetExtension(file.FileName)
                    .ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Only image files are allowed.");
                }

                var fileName =
                    $"{cleanProductName}_{imageCounter}{extension}";

                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = fileName
                };

                _context.ProductImages.Add(productImage);

                imageCounter++;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Images uploaded successfully"
            });
        }

        private bool ProductImageExists(int id)
        {
            return _context.ProductImages.Any(e => e.ProductImageId == id);
        }
    }
}