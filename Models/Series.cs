using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrameForge.Models
{
    public class Series
    {
        public int SeriesId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public int BrandId { get; set; }

        public Brand Brand { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}