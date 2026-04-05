namespace Backend.Exceptions
{
    // ════════════════════════════════════════════════════════════════════════
    // CUSTOM EXCEPTIONS
    // ════════════════════════════════════════════════════════════════════════
    // These are our own exception types for specific error situations.
    // Instead of throwing a generic Exception("something went wrong"),
    // we throw a specific one like NotFoundException("Product not found").
    //
    // The GlobalExceptionMiddleware catches ALL of these and maps them
    // to the correct HTTP status codes automatically.
    //
    // HOW IT WORKS:
    //   Service throws NotFoundException
    //        ↓
    //   GlobalExceptionMiddleware catches it
    //        ↓
    //   Returns HTTP 404 with { "error": "..." } JSON
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Throw when a resource cannot be found in the database.
    /// Maps to HTTP 404 Not Found.
    /// Example: Product with id=99 does not exist.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }

        // Convenience: NotFoundException("Product", 99) → "Product with id 99 not found"
        public NotFoundException(string entityName, int id)
            : base($"{entityName} with id {id} not found") { }
    }

    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Throw when business rule validation fails.
    /// Maps to HTTP 400 Bad Request.
    /// Example: Trying to order more items than are in stock.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Throw when user is not logged in or token is invalid.
    /// Maps to HTTP 401 Unauthorized.
    /// Example: Accessing a protected endpoint without a JWT token.
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Throw when user IS logged in but does NOT have permission.
    /// Maps to HTTP 403 Forbidden.
    /// Example: Customer trying to access Admin dashboard.
    /// </summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }

        // Default message for ownership violations
        public ForbiddenException()
            : base("You do not have permission to perform this action") { }
    }

    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Throw when trying to create something that already exists.
    /// Maps to HTTP 409 Conflict.
    /// Example: Registering with an email that is already used.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }

    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Throw when a banned user tries to log in.
    /// Maps to HTTP 403 Forbidden.
    /// </summary>
    public class AccountBannedException : Exception
    {
        public AccountBannedException(string? reason = null)
            : base(reason != null
                ? $"Your account has been banned. Reason: {reason}"
                : "Your account has been banned. Contact support.") { }
    }
}