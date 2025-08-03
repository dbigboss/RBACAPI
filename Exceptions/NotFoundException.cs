namespace RBACApi.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class NotFoundException : BaseException
{
    public NotFoundException(string message) : base(message)
    {
        ErrorCode = "RESOURCE_NOT_FOUND";
    }

    public NotFoundException(string resourceType, object resourceId) 
        : base($"{resourceType} with ID '{resourceId}' was not found")
    {
        ErrorCode = "RESOURCE_NOT_FOUND";
        ResourceType = resourceType;
        ResourceId = resourceId?.ToString();
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "RESOURCE_NOT_FOUND";
    }

    /// <summary>
    /// Type of resource that was not found
    /// </summary>
    public string? ResourceType { get; set; }

    /// <summary>
    /// ID of the resource that was not found
    /// </summary>
    public string? ResourceId { get; set; }
}