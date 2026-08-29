using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameForge.DTOs
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }
    }
}
