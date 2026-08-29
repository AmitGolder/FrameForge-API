using System.Collections.Generic;

namespace FrameForge.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public List<string> Images { get; set; }

        public int StockQuantity { get; set; }

        public bool IsAvailable { get; set; }


        // Brand
        public int? BrandId { get; set; }
        public string BrandName { get; set; }


        // Scale
        public int? ScaleId { get; set; }
        public string ScaleName { get; set; }


        // Category
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }


        // Series
        public int? SeriesId { get; set; }
        public string SeriesName { get; set; }
    }
}