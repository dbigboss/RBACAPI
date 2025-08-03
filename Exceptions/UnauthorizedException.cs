namespace RBACApi.Exceptions;

/// <summary>
/// Exception thrown when user authentication fails
/// </summary>
public class UnauthorizedException : BaseException
{
    public UnauthorizedException() : base("Authentication is required to access this resource")
    {
        ErrorCode = "UNAUTHORIZED";
    }

    public UnauthorizedException(string message) : base(message)
    {
        ErrorCode = "UNAUTHORIZED";
    }

    public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "UNAUTHORIZED";
    }
}