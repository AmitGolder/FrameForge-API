using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrameForge.Models
{
    public class Scale
    {
        public int ScaleId { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}