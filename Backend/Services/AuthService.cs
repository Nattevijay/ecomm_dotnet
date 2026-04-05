using Backend.DTOs;
using Backend.Exceptions;
using Backend.Helpers;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;

namespace Backend.Services
{
    // ════════════════════════════════════════════════════════════════════════
    // LAYER 2: AUTH SERVICE (Implementation)
    // ════════════════════════════════════════════════════════════════════════
    // This is where ALL authentication business logic lives.
    //
    // RULES THIS SERVICE ENFORCES:
    //   - Email must be unique (no duplicate accounts)
    //   - Password is NEVER stored as plain text (always BCrypt hashed)
    //   - Banned users cannot log in
    //   - Only "Customer" or "Seller" roles can self-register (not Admin)
    //   - Returns a JWT token after register/login so the user stays logged in
    //
    // DEPENDENCIES injected by .NET:
    //   - IUserRepository  → talks to Oracle DB (Layer 1)
    //   - JwtService       → creates JWT tokens
    //   - ILogger          → logs info/errors to console
    // ════════════════════════════════════════════════════════════════════════

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService      _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            JwtService jwtService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtService     = jwtService;
            _logger         = logger;
        }

        // ════════════════════════════════════════════════════════════════════
        // REGISTER
        // ════════════════════════════════════════════════════════════════════
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            _logger.LogInformation("Register attempt for email: {Email}", dto.Email);

            // ── Step 1: Validate role ────────────────────────────────────
            // Only Customer and Seller can self-register
            // Admin accounts must be created by an existing Admin
            var allowedRoles = new[] { UserRoles.Customer, UserRoles.Seller };
            if (!allowedRoles.Contains(dto.Role))
                throw new ValidationException(
                    $"Role must be '{UserRoles.Customer}' or '{UserRoles.Seller}'");

            // ── Step 2: Check email is not already taken ─────────────────
            var emailAlreadyExists = await _userRepository.EmailExistsAsync(dto.Email);
            if (emailAlreadyExists)
                throw new ConflictException(
                    $"An account with email '{dto.Email}' already exists");

            // ── Step 3: Hash the password ────────────────────────────────
            // BCrypt.HashPassword NEVER stores the real password.
            // Even if someone steals your database, they can NOT reverse it.
            // Work factor 12 means it takes ~250ms — slow enough to prevent brute force.
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);

            // ── Step 4: Create the User entity ───────────────────────────
            var user = new User
            {
                Name         = dto.Name.Trim(),
                Email        = dto.Email.ToLower().Trim(),
                PasswordHash = passwordHash,
                Role         = dto.Role,
                IsBanned     = false,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            };

            // ── Step 5: Save to Oracle DB (via Repository) ───────────────
            var savedUser = await _userRepository.AddAsync(user);
            _logger.LogInformation("New user registered: {Email} as {Role}", savedUser.Email, savedUser.Role);

            // ── Step 6: Generate JWT token ───────────────────────────────
            var token = _jwtService.GenerateToken(savedUser);

            // ── Step 7: Return token + user info ─────────────────────────
            return MapToAuthResponse(savedUser, token);
        }

        // ════════════════════════════════════════════════════════════════════
        // LOGIN
        // ════════════════════════════════════════════════════════════════════
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            // ── Step 1: Find user by email ───────────────────────────────
            var user = await _userRepository.GetByEmailAsync(dto.Email.ToLower().Trim());

            // SECURITY: Use same error message for "not found" and "wrong password"
            // Never tell the attacker WHICH one is wrong
            if (user == null)
                throw new UnauthorizedException("Invalid email or password");

            // ── Step 2: Verify password against the stored hash ──────────
            // BCrypt.Verify compares the plain password against the stored hash
            var passwordIsCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!passwordIsCorrect)
            {
                _logger.LogWarning("Failed login for: {Email}", dto.Email);
                throw new UnauthorizedException("Invalid email or password");
            }

            // ── Step 3: Check if account is banned ───────────────────────
            if (user.IsBanned)
            {
                _logger.LogWarning("Banned user attempted login: {Email}", dto.Email);
                throw new AccountBannedException(user.BanReason);
            }

            // ── Step 4: Generate token ───────────────────────────────────
            var token = _jwtService.GenerateToken(user);
            _logger.LogInformation("Login successful: {Email}", dto.Email);

            // ── Step 5: Return token + user info ─────────────────────────
            return MapToAuthResponse(user, token);
        }

        // ════════════════════════════════════════════════════════════════════
        // GET PROFILE (for /api/auth/me)
        // ════════════════════════════════════════════════════════════════════
        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new NotFoundException("User", userId);

            return MapToProfileDto(user);
        }

        // ════════════════════════════════════════════════════════════════════
        // UPDATE PROFILE
        // ════════════════════════════════════════════════════════════════════
        public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new NotFoundException("User", userId);

            // Only update the allowed fields
            user.Name      = dto.Name.Trim();
            user.Phone     = dto.Phone?.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            var updatedUser = await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Profile updated for user {UserId}", userId);
            return MapToProfileDto(updatedUser);
        }

        // ════════════════════════════════════════════════════════════════════
        // CHANGE PASSWORD
        // ════════════════════════════════════════════════════════════════════
        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new NotFoundException("User", userId);

            // ── Verify current password first ────────────────────────────
            var currentPasswordCorrect = BCrypt.Net.BCrypt.Verify(
                dto.CurrentPassword,
                user.PasswordHash);

            if (!currentPasswordCorrect)
                throw new ValidationException("Current password is incorrect");

            // ── Hash and save new password ───────────────────────────────
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);
            user.UpdatedAt    = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Password changed for user {UserId}", userId);
        }

        // ════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS — Mapping entity → DTO
        // ════════════════════════════════════════════════════════════════════

        // Converts a User + token into the AuthResponseDto
        private static AuthResponseDto MapToAuthResponse(User user, string token)
        {
            return new AuthResponseDto
            {
                Token = token,
                Id    = user.Id,
                Name  = user.Name,
                Email = user.Email,
                Role  = user.Role
            };
        }

        // Converts a User into the safe profile DTO (no password hash!)
        private static UserProfileDto MapToProfileDto(User user)
        {
            return new UserProfileDto
            {
                Id              = user.Id,
                Name            = user.Name,
                Email           = user.Email,
                Role            = user.Role,
                IsBanned        = user.IsBanned,
                ProfileImageUrl = user.ProfileImageUrl,
                Phone           = user.Phone,
                CreatedAt       = user.CreatedAt
            };
        }
    }
}