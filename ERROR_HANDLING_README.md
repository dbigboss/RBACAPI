# 🛡️ Global Error Handling Implementation

## Overview

This RBAC API now includes comprehensive global error handling with custom exceptions and structured error responses.

## 🏗️ Architecture

### Components Added:

1. **GlobalExceptionHandlingMiddleware** - Catches all unhandled exceptions
2. **Custom Exception Classes** - Typed exceptions for different scenarios
3. **ExceptionService** - Helper service for throwing exceptions
4. **TestErrorController** - Endpoints for testing error handling

## 📁 File Structure

```
RBACApi/
├── Middleware/
│   └── GlobalExceptionHandlingMiddleware.cs
├── Exceptions/
│   ├── BaseException.cs
│   ├── ValidationException.cs
│   ├── NotFoundException.cs
│   ├── UnauthorizedException.cs
│   ├── ForbiddenException.cs
│   ├── ConflictException.cs
│   └── BadRequestException.cs
├── Services/
│   ├── IExceptionService.cs
│   └── ExceptionService.cs
└── Controllers/
    └── TestErrorController.cs
```

## 🎯 Custom Exceptions

### 1. ValidationException
- **HTTP Status**: 400 Bad Request
- **Use Case**: Input validation failures
- **Features**: Supports field-specific error messages

```csharp
// Usage example
var errors = new Dictionary<string, string[]>
{
    { "Email", new[] { "Email is required", "Email format is invalid" } },
    { "Password", new[] { "Password must be at least 8 characters" } }
};
throw new ValidationException("Validation failed", errors);
```

### 2. NotFoundException
- **HTTP Status**: 404 Not Found
- **Use Case**: Resource not found
- **Features**: Includes resource type and ID

```csharp
// Usage examples
throw new NotFoundException("User", 123);
throw new NotFoundException("User with ID '123' was not found");
```

### 3. UnauthorizedException
- **HTTP Status**: 401 Unauthorized
- **Use Case**: Authentication failures
- **Features**: Customizable message

```csharp
// Usage examples
throw new UnauthorizedException(); // Default message
throw new UnauthorizedException("Invalid token provided");
```

### 4. ForbiddenException
- **HTTP Status**: 403 Forbidden
- **Use Case**: Authorization failures
- **Features**: Supports resource and action context

```csharp
// Usage examples
throw new ForbiddenException(); // Default message
throw new ForbiddenException("Admin Panel", "access");
```

### 5. ConflictException
- **HTTP Status**: 409 Conflict
- **Use Case**: Resource conflicts (e.g., duplicate email)
- **Features**: Includes conflict reason

```csharp
// Usage example
throw new ConflictException("User", "Email address is already registered");
```

### 6. BadRequestException
- **HTTP Status**: 400 Bad Request
- **Use Case**: Malformed requests
- **Features**: Includes invalid field and expected format

```csharp
// Usage example
var exception = new BadRequestException("Invalid input format")
{
    InvalidField = "DateOfBirth",
    ExpectedFormat = "YYYY-MM-DD"
};
throw exception;
```

## 🚀 Usage Examples

### Using ExceptionService (Recommended)

```csharp
public class UsersController : ControllerBase
{
    private readonly IExceptionService _exceptionService;

    public UsersController(IExceptionService exceptionService)
    {
        _exceptionService = exceptionService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
        {
            _exceptionService.ThrowNotFound("User", id);
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(dto.Email))
        {
            _exceptionService.ThrowConflict("Email address is already registered", "User", "Email conflict");
        }

        // Validate input
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Email", new[] { "Email is required" } }
            };
            _exceptionService.ThrowValidation("Validation failed", errors);
        }

        // Create user logic...
        return Ok();
    }
}
```

### Direct Exception Throwing

```csharp
// Direct usage without service
if (user == null)
{
    throw new NotFoundException("User", id);
}

if (!User.IsInRole("Admin"))
{
    throw new ForbiddenException("Admin Panel", "access");
}
```

## 📋 Error Response Format

All errors return a consistent JSON structure following the RFC 7807 Problem Details specification:

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "User with ID '123' was not found",
  "instance": "/api/users/123",
  "traceId": "00-1234567890abcdef-1234567890abcdef-01",
  "timestamp": "2024-01-01T12:00:00.000Z"
}
```

### Validation Error Response

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation Error",
  "status": 400,
  "detail": "Validation failed",
  "instance": "/api/users",
  "errors": {
    "Email": ["Email is required", "Email format is invalid"],
    "Password": ["Password must be at least 8 characters"]
  },
  "traceId": "00-1234567890abcdef-1234567890abcdef-01",
  "timestamp": "2024-01-01T12:00:00.000Z"
}
```

### Development Environment Extras

In development mode, additional debugging information is included:

```json
{
  "type": "https://httpstatuses.com/500",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred. Please try again later.",
  "instance": "/api/users/123",
  "stackTrace": "...",
  "source": "MyApplication",
  "innerException": "...",
  "traceId": "00-1234567890abcdef-1234567890abcdef-01",
  "timestamp": "2024-01-01T12:00:00.000Z"
}
```

## 🧪 Testing the Error Handling

### Test Endpoints Available

All test endpoints are available under `/api/TestError/`:

1. **GET** `/api/TestError/not-found` - Tests NotFoundException
2. **POST** `/api/TestError/validation-error` - Tests ValidationException
3. **GET** `/api/TestError/unauthorized` - Tests UnauthorizedException
4. **GET** `/api/TestError/forbidden` - Tests ForbiddenException
5. **POST** `/api/TestError/conflict` - Tests ConflictException
6. **POST** `/api/TestError/bad-request` - Tests BadRequestException
7. **GET** `/api/TestError/system-error` - Tests unhandled system exception
8. **GET** `/api/TestError/argument-null` - Tests ArgumentNullException
9. **GET** `/api/TestError/invalid-operation` - Tests InvalidOperationException

### Testing with curl

```bash
# Test NotFoundException
curl -X GET "https://localhost:7000/api/TestError/not-found"

# Test ValidationException
curl -X POST "https://localhost:7000/api/TestError/validation-error"

# Test UnauthorizedException
curl -X GET "https://localhost:7000/api/TestError/unauthorized"

# Test system error
curl -X GET "https://localhost:7000/api/TestError/system-error"
```

## 🔧 Middleware Configuration

The middleware is registered early in the pipeline in `Program.cs`:

```csharp
// Add global exception handling middleware (should be one of the first middleware)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

## ⚡ Performance Considerations

- **Minimal Overhead**: Middleware only activates when exceptions occur
- **Efficient Logging**: Structured logging with appropriate log levels
- **Memory Efficient**: No unnecessary object allocations in normal flow
- **Fast Response**: Quick error response generation

## 🛡️ Security Features

- **Information Hiding**: Sensitive stack traces only shown in development
- **Consistent Responses**: All errors follow the same format
- **Trace ID**: Unique identifier for tracking errors across logs
- **No Sensitive Data**: Custom exceptions avoid exposing internal details

## 📊 Monitoring & Logging

All exceptions are automatically logged with:
- **Error Level**: For unhandled exceptions
- **Warning Level**: For custom business exceptions
- **Trace ID**: For correlation with client responses
- **Request Context**: Path, method, user information

## 🎯 Best Practices

1. **Use Custom Exceptions**: Prefer custom exceptions over generic ones
2. **Use ExceptionService**: Utilize the helper service for consistency
3. **Provide Context**: Include relevant information (resource type, ID, etc.)
4. **Avoid Over-catching**: Let the middleware handle exceptions
5. **Log Appropriately**: Use structured logging for better monitoring

## 🔄 Future Enhancements

Potential improvements to consider:

1. **Rate Limiting**: Add rate limiting for error endpoints
2. **Error Aggregation**: Collect and analyze error patterns
3. **Circuit Breaker**: Implement circuit breaker pattern for external services
4. **Custom Error Codes**: Add application-specific error codes
5. **Localization**: Support for multiple languages in error messages

---

**✅ Global error handling is now fully implemented and ready for use!**