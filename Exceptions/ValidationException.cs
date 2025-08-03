namespace RBACApi.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : BaseException
{
    public ValidationException(string message) : base(message)
    {
        ErrorCode = "VALIDATION_ERROR";
    }

    public ValidationException(string message, Dictionary<string, string[]> errors) : base(message)
    {
        ErrorCode = "VALIDATION_ERROR";
        Errors = errors;
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "VALIDATION_ERROR";
    }

    /// <summary>
    /// Dictionary of field validation errors
    /// </summary>
    public Dictionary<string, string[]> Errors { get; set; } = new();

    /// <summary>
    /// Add a validation error for a specific field
    /// </summary>
    public void AddError(string field, string error)
    {
        if (!Errors.ContainsKey(field))
        {
            Errors[field] = new string[] { error };
        }
        else
        {
            var existingErrors = Errors[field].ToList();
            existingErrors.Add(error);
            Errors[field] = existingErrors.ToArray();
        }
    }

    /// <summary>
    /// Add multiple validation errors for a specific field
    /// </summary>
    public void AddErrors(string field, IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            AddError(field, error);
        }
    }
}