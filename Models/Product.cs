using System.ComponentModel.DataAnnotations;

namespace RBACApi.Models;

public class Product
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    
    public ProductStatus Status { get; set; } = ProductStatus.Pending;
    
    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;
    
    public ApplicationUser? CreatedByUser { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ApprovedAt { get; set; }
    
    public string? ApprovedByUserId { get; set; }
    
    public ApplicationUser? ApprovedByUser { get; set; }
}

public enum ProductStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}