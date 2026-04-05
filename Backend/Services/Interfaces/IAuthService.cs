using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    // ════════════════════════════════════════════════════════════════════════
    // LAYER 2: AUTH SERVICE INTERFACE
    // ════════════════════════════════════════════════════════════════════════
    // Defines WHAT the auth service can do — the business logic contract.
    // The AuthController depends on this interface, not the concrete class.
    //
    // REGISTER in Program.cs:
    //   builder.Services.AddScoped<IAuthService, AuthService>();
    // ════════════════════════════════════════════════════════════════════════

    public interface IAuthService
    {
        /// <summary>
        /// Register a new user.
        /// Validates email uniqueness, hashes password, saves user, returns token.
        /// Throws ConflictException if email already exists.
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

        /// <summary>
        /// Log in an existing user.
        /// Validates email exists, checks password hash, checks if banned.
        /// Returns JWT token on success.
        /// Throws UnauthorizedException if credentials are wrong.
        /// Throws AccountBannedException if user is banned.
        /// </summary>
        Task<AuthResponseDto> LoginAsync(LoginDto dto);

        /// <summary>
        /// Get a user's profile by their ID (from JWT token).
        /// Used by GET /api/auth/me
        /// </summary>
        Task<UserProfileDto> GetProfileAsync(int userId);

        /// <summary>
        /// Update a user's name and phone.
        /// </summary>
        Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);

        /// <summary>
        /// Change a user's password.
        /// Verifies current password before allowing change.
        /// </summary>
        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    }
}