using Microsoft.AspNetCore.Mvc;
using RBACApi.Exceptions;
using RBACApi.Services;

namespace RBACApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestErrorController : ControllerBase
{
    private readonly IExceptionService _exceptionService;

    public TestErrorController(IExceptionService exceptionService)
    {
        _exceptionService = exceptionService;
    }

    /// <summary>
    /// Test endpoint for NotFoundException
    /// </summary>
    [HttpGet("not-found")]
    public IActionResult TestNotFound()
    {
        _exceptionService.ThrowNotFound("User", 123);
        return Ok(); // This will never be reached
    }

    /// <summary>
    /// Test endpoint for ValidationException
    /// </summary>
    [HttpPost("validation-error")]
    public IActionResult TestValidationError()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required", "Email format is invalid" } },
            { "Password", new[] { "Password must be at least 8 characters" } }
        };

        _exceptionService.ThrowValidation("Validation failed", errors);
        return Ok(); // This will never be reached
    }

    /// <summary>
    /// Test endpoint for UnauthorizedException
    /// </summary>
    [HttpGet("unauthorized")]
    public IActionResult TestUnauthorized()
    {
        _exceptionService.ThrowUnauthorized("Invalid token provided");
        return Ok(); // This will never be reached
    }

    /// <summary>
    /// Test endpoint for ForbiddenException
    /// </summary>
    [HttpGet("forbidden")]
    public IActionResult TestForbidden()
    {
        _exceptionService.ThrowForbidden(resource: "Admin Panel", action: "access");
        return Ok(); // This will never be reached
    }

    /// <summary>
    /// Test endpoint for ConflictException
    /// </summary>
    [HttpPost("conflict")]
    public IActionResult TestConflict()
    {
        _exceptionService.ThrowConflict("Email already exists", "User", "Email address is already registered");
        return Ok(); // This will never be reached
    }

    /// <summary>
    /// Test endpoint for BadRequestException
    /// </summary>
    [HttpPost("bad-request")]
    public IActionResult TestBadRequest()
    {
        _exceptionService.ThrowBadRequest("Invalid input format", "DateOfBirth", "YYYY-MM-DD");
        return Ok(); // This will never be reached
    }

    /// <summary>
    /// Test endpoint for unhandled system exception
    /// </summary>
    [HttpGet("system-error")]
    public IActionResult TestSystemError()
    {
        // This will cause a system exception that will be caught by the middleware
        var result = 10 / int.Parse("0");
        return Ok(result);
    }

    /// <summary>
    /// Test endpoint for ArgumentNullException
    /// </summary>
    [HttpGet("argument-null")]
    public IActionResult TestArgumentNull()
    {
        string? nullString = null;
        // This will cause an ArgumentNullException
        var length = nullString!.Length;
        return Ok(length);
    }

    /// <summary>
    /// Test endpoint for InvalidOperationException
    /// </summary>
    [HttpGet("invalid-operation")]
    public IActionResult TestInvalidOperation()
    {
        var list = new List<int>();
        // This will cause an InvalidOperationException
        var first = list.First();
        return Ok(first);
    }
}