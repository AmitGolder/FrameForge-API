using System.Collections.Generic;

namespace FrameForge.DTOs
{
    public class PlaceOrderDto
    {
        public string CustomerName { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        public List<OrderItemDto> Items { get; set; }
    }
}