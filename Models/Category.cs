using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrameForge.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}