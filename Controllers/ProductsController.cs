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
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Policy = "UserOrAbove")]
    public async Task<IActionResult> GetProducts([FromQuery] bool includesPending = false)
    {
        var query = _context.Products
            .Include(p => p.CreatedByUser)
            .Include(p => p.ApprovedByUser)
            .AsQueryable();

        if (!includesPending)
        {
            query = query.Where(p => p.Status == ProductStatus.Approved);
        }

        var products = await query
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                Status = p.Status,
                CreatedByUserId = p.CreatedByUserId,
                CreatedByUserName = p.CreatedByUser!.UserName!,
                CreatedAt = p.CreatedAt,
                ApprovedAt = p.ApprovedAt,
                ApprovedByUserId = p.ApprovedByUserId,
                ApprovedByUserName = p.ApprovedByUser != null ? p.ApprovedByUser.UserName : null
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "UserOrAbove")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.CreatedByUser)
            .Include(p => p.ApprovedByUser)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            Status = product.Status,
            CreatedByUserId = product.CreatedByUserId,
            CreatedByUserName = product.CreatedByUser!.UserName!,
            CreatedAt = product.CreatedAt,
            ApprovedAt = product.ApprovedAt,
            ApprovedByUserId = product.ApprovedByUserId,
            ApprovedByUserName = product.ApprovedByUser?.UserName
        };

        return Ok(productDto);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var product = new Product
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            Stock = model.Stock,
            CreatedByUserId = userId,
            Status = ProductStatus.Pending
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (product.CreatedByUserId != userId && !User.IsInRole(Roles.SuperAdmin))
            return Forbid();

        if (model.Name != null)
            product.Name = model.Name;
        if (model.Description != null)
            product.Description = model.Description;
        if (model.Price.HasValue)
            product.Price = model.Price.Value;
        if (model.Stock.HasValue)
            product.Stock = model.Stock.Value;

        product.Status = ProductStatus.Pending;
        product.ApprovedAt = null;
        product.ApprovedByUserId = null;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (product.CreatedByUserId != userId && !User.IsInRole(Roles.SuperAdmin))
            return Forbid();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/approve")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> ApproveProduct(int id, [FromBody] ApproveProductDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        if (product.Status != ProductStatus.Pending)
            return BadRequest("Only pending products can be approved or rejected");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        product.Status = model.Status;
        product.ApprovedAt = DateTime.UtcNow;
        product.ApprovedByUserId = userId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("pending")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> GetPendingProducts()
    {
        var products = await _context.Products
            .Include(p => p.CreatedByUser)
            .Where(p => p.Status == ProductStatus.Pending)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                Status = p.Status,
                CreatedByUserId = p.CreatedByUserId,
                CreatedByUserName = p.CreatedByUser!.UserName!,
                CreatedAt = p.CreatedAt,
                ApprovedAt = p.ApprovedAt,
                ApprovedByUserId = p.ApprovedByUserId,
                ApprovedByUserName = null
            })
            .ToListAsync();

        return Ok(products);
    }
}