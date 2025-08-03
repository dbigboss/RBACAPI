namespace RBACApi.Exceptions;

/// <summary>
/// Base exception class for all custom application exceptions
/// </summary>
public abstract class BaseException : Exception
{
    protected BaseException(string message) : base(message) { }

    protected BaseException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Additional error details that can be included in the response
    /// </summary>
    public virtual object? ErrorDetails { get; set; }

    /// <summary>
    /// Error code for categorizing different types of errors
    /// </summary>
    public virtual string? ErrorCode { get; set; }
}