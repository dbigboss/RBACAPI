using RBACApi.Exceptions;

namespace RBACApi.Services;

/// <summary>
/// Service for handling and throwing custom exceptions
/// </summary>
public class ExceptionService : IExceptionService
{
    public void ThrowNotFound(string resourceType, object resourceId)
    {
        throw new NotFoundException(resourceType, resourceId);
    }

    public void ThrowValidation(string message, Dictionary<string, string[]>? errors = null)
    {
        if (errors != null)
        {
            throw new ValidationException(message, errors);
        }
        throw new ValidationException(message);
    }

    public void ThrowUnauthorized(string? message = null)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new UnauthorizedException();
        }
        throw new UnauthorizedException(message);
    }

    public void ThrowForbidden(string? message = null, string? resource = null, string? action = null)
    {
        if (!string.IsNullOrEmpty(resource) && !string.IsNullOrEmpty(action))
        {
            throw new ForbiddenException(resource, action);
        }
        
        if (string.IsNullOrEmpty(message))
        {
            throw new ForbiddenException();
        }
        
        throw new ForbiddenException(message);
    }

    public void ThrowConflict(string message, string? resourceType = null, string? conflictReason = null)
    {
        if (!string.IsNullOrEmpty(resourceType) && !string.IsNullOrEmpty(conflictReason))
        {
            throw new ConflictException(resourceType, conflictReason);
        }
        throw new ConflictException(message);
    }

    public void ThrowBadRequest(string message, string? invalidField = null, string? expectedFormat = null)
    {
        var exception = new BadRequestException(message)
        {
            InvalidField = invalidField,
            ExpectedFormat = expectedFormat
        };
        throw exception;
    }
}