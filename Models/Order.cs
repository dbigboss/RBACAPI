using System.ComponentModel.DataAnnotations;

namespace RBACApi.Models;

public class Order
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public ApplicationUser? User { get; set; }
    
    public List<OrderItem> OrderItems { get; set; } = new();
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }
    
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    
    public int OrderId { get; set; }
    
    public Order? Order { get; set; }
    
    public int ProductId { get; set; }
    
    public Product? Product { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalPrice { get; set; }
}

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Cancelled = 3
}