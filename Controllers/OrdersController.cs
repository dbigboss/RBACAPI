using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RBACApi.Constants;
using RBACApi.Data;
using RBACApi.DTOs;
using RBACApi.Models;
using System.Security.Claims;

namespace RBACApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Policy = "UserOrAbove")]
    public async Task<IActionResult> GetOrders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SuperAdmin);

        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(o => o.UserId == userId);
        }

        var orders = await query
            .Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                UserName = o.User!.UserName!,
                Items = o.OrderItems.Select(oi => new OrderItemDetailDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product!.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList(),
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                CompletedAt = o.CompletedAt
            })
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "UserOrAbove")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SuperAdmin);

        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        if (!isAdmin && order.UserId != userId)
            return Forbid();

        var orderDto = new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            UserName = order.User!.UserName!,
            Items = order.OrderItems.Select(oi => new OrderItemDetailDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product!.Name,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList(),
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            CompletedAt = order.CompletedAt
        };

        return Ok(orderDto);
    }

    [HttpPost]
    [Authorize(Policy = "UserOrAbove")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var productIds = model.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id) && p.Status == ProductStatus.Approved)
                .ToListAsync();

            if (products.Count != productIds.Count)
            {
                return BadRequest("One or more products are not available or not approved");
            }

            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in model.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);

                if (product.Stock < item.Quantity)
                {
                    return BadRequest($"Insufficient stock for product {product.Name}. Available: {product.Stock}, Requested: {item.Quantity}");
                }

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * item.Quantity
                };

                orderItems.Add(orderItem);
                totalAmount += orderItem.TotalPrice;

                product.Stock -= item.Quantity;
            }

            _context.OrderItems.AddRange(orderItems);
            order.TotalAmount = totalAmount;
            order.Status = OrderStatus.Processing;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new { orderId = order.Id });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "An error occurred while processing the order");
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            return BadRequest("Cannot update status of completed or cancelled orders");

        order.Status = status;
        
        if (status == OrderStatus.Completed)
        {
            order.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "UserOrAbove")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SuperAdmin);

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        if (!isAdmin && order.UserId != userId)
            return Forbid();

        if (order.Status == OrderStatus.Completed)
            return BadRequest("Cannot cancel completed orders");

        if (order.Status == OrderStatus.Cancelled)
            return BadRequest("Order is already cancelled");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                {
                    item.Product.Stock += item.Quantity;
                }
            }

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return NoContent();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "An error occurred while cancelling the order");
        }
    }
}