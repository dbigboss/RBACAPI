namespace RBACApi.Exceptions;

/// <summary>
/// Exception thrown when a request conflicts with the current state of the resource
/// </summary>
public class ConflictException : BaseException
{
    public ConflictException(string message) : base(message)
    {
        ErrorCode = "RESOURCE_CONFLICT";
    }

    public ConflictException(string resourceType, string conflictReason) 
        : base($"Conflict with {resourceType}: {conflictReason}")
    {
        ErrorCode = "RESOURCE_CONFLICT";
        ResourceType = resourceType;
        ConflictReason = conflictReason;
    }

    public ConflictException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "RESOURCE_CONFLICT";
    }

    /// <summary>
    /// Type of resource that has a conflict
    /// </summary>
    public string? ResourceType { get; set; }

    /// <summary>
    /// Reason for the conflict
    /// </summary>
    public string? ConflictReason { get; set; }
}