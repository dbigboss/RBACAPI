using RBACApi.Exceptions;

namespace RBACApi.Services;

/// <summary>
/// Service interface for handling and throwing custom exceptions
/// </summary>
public interface IExceptionService
{
    /// <summary>
    /// Throws a NotFoundException with formatted message
    /// </summary>
    void ThrowNotFound(string resourceType, object resourceId);

    /// <summary>
    /// Throws a ValidationException with field errors
    /// </summary>
    void ThrowValidation(string message, Dictionary<string, string[]>? errors = null);

    /// <summary>
    /// Throws an UnauthorizedException
    /// </summary>
    void ThrowUnauthorized(string? message = null);

    /// <summary>
    /// Throws a ForbiddenException
    /// </summary>
    void ThrowForbidden(string? message = null, string? resource = null, string? action = null);

    /// <summary>
    /// Throws a ConflictException
    /// </summary>
    void ThrowConflict(string message, string? resourceType = null, string? conflictReason = null);

    /// <summary>
    /// Throws a BadRequestException
    /// </summary>
    void ThrowBadRequest(string message, string? invalidField = null, string? expectedFormat = null);
}