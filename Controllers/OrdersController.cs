using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FrameForge.Data;
using FrameForge.DTOs;
using FrameForge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrameForge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly FrameForgeDbContext _context;

        public OrdersController(FrameForgeDbContext context)
        {
            _context = context;
        }


        // =========================
        // GET ALL ORDERS - ADMIN
        // =========================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }


        // =========================
        // GET LOGGED-IN USER ORDERS
        // =========================

        // GET: api/Orders/my-orders

        [Authorize]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier
            );

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(
                userIdClaim.Value
            );


            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.OrderId,
                    o.Status,
                    o.TotalAmount,
                    o.OrderDate
                })
                .ToListAsync();


            return Ok(orders);
        }


        // =========================
        // GET SINGLE ORDER - ADMIN
        // =========================

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(
                    o => o.OrderId == id
                );

            if (order == null)
            {
                return NotFound();
            }


            var result = new
            {
                order.OrderId,
                order.CustomerName,
                order.Address,
                order.Phone,
                order.Status,
                order.TotalAmount,
                order.OrderDate,

                Items = order.OrderItems.Select(item =>
                    new
                    {
                        item.OrderItemId,
                        item.ProductId,

                        ProductName = item.Product.Name,

                        item.Quantity,
                        item.Price,

                        Image = item.Product.ProductImages
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault()
                    })
            };


            return Ok(result);
        }


        // =========================
        // UPDATE ORDER STATUS - ADMIN
        // =========================

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(
            int id,
            [FromBody] UpdateOrderStatusDto dto)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(
                    o => o.OrderId == id
                );

            if (order == null)
            {
                return NotFound();
            }


            order.Status = dto.Status;

            await _context.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Order status updated successfully"
            });
        }


        // =========================
        // PLACE ORDER - LOGGED-IN USER
        // =========================

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(
            [FromBody] PlaceOrderDto dto)
        {
            // Get logged-in user's ID from JWT

            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier
            );

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(
                userIdClaim.Value
            );


            // Validate order

            if (dto == null ||
                dto.Items == null ||
                !dto.Items.Any())
            {
                return BadRequest(
                    "Order is empty."
                );
            }


            // Check products and stock

            foreach (var item in dto.Items)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(
                        p => p.ProductId ==
                             item.ProductId
                    );

                if (product == null)
                {
                    return BadRequest(
                        $"Product {item.ProductId} not found."
                    );
                }


                if (!product.IsAvailable ||
                    product.StockQuantity <
                    item.Quantity)
                {
                    return BadRequest(
                        $"{product.Name} is out of stock."
                    );
                }


                product.StockQuantity -=
                    item.Quantity;


                if (product.StockQuantity <= 0)
                {
                    product.StockQuantity = 0;

                    product.IsAvailable = false;
                }
            }


            // Calculate total

            decimal totalAmount =
                dto.Items.Sum(
                    item =>
                        item.Price *
                        item.Quantity
                );


            // Create order

            var order = new Order
            {
                UserId = userId,

                CustomerName =
                    dto.CustomerName,

                Address =
                    dto.Address,

                Phone =
                    dto.Phone,

                Status =
                    "Pending",

                TotalAmount =
                    totalAmount,

                OrderDate =
                    DateTime.Now,


                OrderItems =
                    dto.Items.Select(item =>
                        new OrderItem
                        {
                            ProductId =
                                item.ProductId,

                            Quantity =
                                item.Quantity,

                            Price =
                                item.Price
                        }
                    ).ToList()
            };


            _context.Orders.Add(
                order
            );

            await _context.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Order placed successfully",

                orderId =
                    order.OrderId
            });
        }


        // =========================
        // TRACK ORDER - PUBLIC
        // =========================

        [HttpPost("track")]
        public async Task<IActionResult> TrackOrder(
            [FromBody] TrackOrderDto dto)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(
                            p => p.ProductImages
                        )

                .FirstOrDefaultAsync(o =>
                    o.OrderId ==
                    dto.OrderId &&

                    o.Phone ==
                    dto.Phone
                );


            if (order == null)
            {
                return NotFound(
                    new
                    {
                        message =
                            "Order not found"
                    }
                );
            }


            var result = new
            {
                order.OrderId,

                order.CustomerName,

                order.Address,

                order.Phone,

                order.Status,

                order.TotalAmount,

                order.OrderDate,


                Items =
                    order.OrderItems.Select(item =>
                        new
                        {
                            item.OrderItemId,

                            item.ProductId,

                            ProductName =
                                item.Product.Name,

                            item.Quantity,

                            item.Price,


                            Image =
                                item.Product
                                    .ProductImages
                                    .Select(
                                        i => i.ImageUrl
                                    )
                                    .FirstOrDefault()
                        }
                    )
            };


            return Ok(result);
        }
    }
}