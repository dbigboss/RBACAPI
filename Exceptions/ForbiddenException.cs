namespace RBACApi.Exceptions;

/// <summary>
/// Exception thrown when user is authenticated but lacks required permissions
/// </summary>
public class ForbiddenException : BaseException
{
    public ForbiddenException() : base("You do not have permission to access this resource")
    {
        ErrorCode = "FORBIDDEN";
    }

    public ForbiddenException(string message) : base(message)
    {
        ErrorCode = "FORBIDDEN";
    }

    public ForbiddenException(string resource, string action) 
        : base($"You do not have permission to {action} {resource}")
    {
        ErrorCode = "FORBIDDEN";
        Resource = resource;
        Action = action;
    }

    public ForbiddenException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "FORBIDDEN";
    }

    /// <summary>
    /// The resource that access was denied to
    /// </summary>
    public string? Resource { get; set; }

    /// <summary>
    /// The action that was denied
    /// </summary>
    public string? Action { get; set; }
}