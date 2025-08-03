namespace RBACApi.Exceptions;

/// <summary>
/// Exception thrown when the request is malformed or invalid
/// </summary>
public class BadRequestException : BaseException
{
    public BadRequestException(string message) : base(message)
    {
        ErrorCode = "BAD_REQUEST";
    }

    public BadRequestException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "BAD_REQUEST";
    }

    /// <summary>
    /// Additional details about what made the request invalid
    /// </summary>
    public string? InvalidField { get; set; }

    /// <summary>
    /// Expected value or format
    /// </summary>
    public string? ExpectedFormat { get; set; }
}