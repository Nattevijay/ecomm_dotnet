namespace Backend.DTOs
{
    // ════════════════════════════════════════════════════════════════════════
    // AUTH DTOs
    // ════════════════════════════════════════════════════════════════════════
    // DTOs = Data Transfer Objects
    // These are simple classes that define WHAT DATA comes IN (requests)
    // and what goes OUT (responses) through the API.
    // They are NOT stored in the database — they only travel over the network.
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// INPUT: What the client sends when registering a new user.
    /// POST /api/auth/register
    /// Body: { "name": "John", "email": "john@test.com", "password": "Test@123", "role": "Customer" }
    /// </summary>
    public class RegisterDto
    {
        public string Name     { get; set; } = string.Empty;
        public string Email    { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Optional: "Customer" or "Seller" (Admin cannot self-register)
        // Defaults to "Customer" if not provided
        public string Role { get; set; } = "Customer";
    }

    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// INPUT: What the client sends when logging in.
    /// POST /api/auth/login
    /// Body: { "email": "john@test.com", "password": "Test@123" }
    /// </summary>
    public class LoginDto
    {
        public string Email    { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// OUTPUT: What the API returns after successful register or login.
    /// Contains the JWT token + basic user info.
    /// The client (React) stores this token in Redux state + localStorage.
    /// </summary>
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int    Id    { get; set; }
        public string Name  { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role  { get; set; } = string.Empty;
    }

    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// OUTPUT: Safe user profile info (NO password hash, NO sensitive data).
    /// Used for GET /api/auth/me and admin user lists.
    /// </summary>
    public class UserProfileDto
    {
        public int      Id             { get; set; }
        public string   Name           { get; set; } = string.Empty;
        public string   Email          { get; set; } = string.Empty;
        public string   Role           { get; set; } = string.Empty;
        public bool     IsBanned       { get; set; }
        public string?  ProfileImageUrl{ get; set; }
        public string?  Phone          { get; set; }
        public DateTime CreatedAt      { get; set; }
    }

    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// INPUT: What the client sends to update their profile.
    /// PUT /api/auth/profile
    /// </summary>
    public class UpdateProfileDto
    {
        public string  Name  { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// INPUT: What the client sends to change their password.
    /// PUT /api/auth/change-password
    /// </summary>
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword     { get; set; } = string.Empty;
    }
}