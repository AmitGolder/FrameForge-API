using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrameForge.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Range(1, 1000000)]
        public decimal Price { get; set; }

        [Range(0, 100000)]
        public int StockQuantity { get; set; }

        public bool IsAvailable { get; set; }

        public DateTime CreatedAt { get; set; }

        // Brand
        public int? BrandId { get; set; }
        public Brand Brand { get; set; }

        // Scale
        public int? ScaleId { get; set; }
        public Scale Scale { get; set; }

        // Category
        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        // Series
        public int? SeriesId { get; set; }
        public Series Series { get; set; }

        // Images
        public ICollection<ProductImage> ProductImages { get; set; }
    }
}