using System;
using System.Collections.Generic;

namespace FrameForge.Models
{
    public class Order
    {
        public int OrderId { get; set; }


        // User
        public int UserId { get; set; }

        public User User { get; set; }


        // Customer Details
        public string CustomerName { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }


        // Order Details
        public string Status { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; }


        // Order Items
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}