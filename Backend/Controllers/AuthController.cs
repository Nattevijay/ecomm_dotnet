using Backend.DTOs;
using Backend.Helpers;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    // ════════════════════════════════════════════════════════════════════════
    // LAYER 3: AUTH CONTROLLER
    // ════════════════════════════════════════════════════════════════════════
    // The Controller is the DOOR to your API.
    // It receives HTTP requests and returns HTTP responses.
    //
    // WHAT THE CONTROLLER DOES:
    //   ✅ Receives the HTTP request (reads body, headers, URL params)
    //   ✅ Calls the Service (business logic)
    //   ✅ Returns the correct HTTP response (200, 201, 400, 401 etc.)
    //
    // WHAT THE CONTROLLER DOES NOT DO:
    //   ❌ No database code (that's the Repository's job)
    //   ❌ No business logic like hashing passwords (that's the Service's job)
    //   ❌ No manual validation (FluentValidation handles that)
    //
    // BASE URL: /api/auth
    //   POST   /api/auth/register         → Register new user
    //   POST   /api/auth/login            → Login
    //   GET    /api/auth/me               → Get my profile (requires login)
    //   PUT    /api/auth/profile          → Update my profile (requires login)
    //   PUT    /api/auth/change-password  → Change password (requires login)
    // ════════════════════════════════════════════════════════════════════════

    [ApiController]
    [Route("api/[controller]")]  // → /api/auth
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly JwtService   _jwtService;

        // .NET automatically injects these — no "new" keyword needed
        public AuthController(IAuthService authService, JwtService jwtService)
        {
            _authService = authService;
            _jwtService  = jwtService;
        }

        // ════════════════════════════════════════════════════════════════════
        // POST /api/auth/register
        // ════════════════════════════════════════════════════════════════════
        /// <summary>Register a new Customer or Seller account.</summary>
        /// <remarks>
        /// Sample request body:
        ///
        ///     POST /api/auth/register
        ///     {
        ///       "name": "John Doe",
        ///       "email": "john@example.com",
        ///       "password": "Secret@123",
        ///       "role": "Customer"
        ///     }
        ///
        /// Returns: JWT token + user info on success.
        /// </remarks>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // FluentValidation already ran — if we're here, the DTO is valid
            // Just call the service and return the result
            var result = await _authService.RegisterAsync(dto);

            // 201 Created (not 200 OK) — a new resource was created
            return StatusCode(StatusCodes.Status201Created, result);
        }

        // ════════════════════════════════════════════════════════════════════
        // POST /api/auth/login
        // ════════════════════════════════════════════════════════════════════
        /// <summary>Login with email and password. Returns a JWT token.</summary>
        /// <remarks>
        /// Sample request body:
        ///
        ///     POST /api/auth/login
        ///     {
        ///       "email": "john@example.com",
        ///       "password": "Secret@123"
        ///     }
        ///
        /// Returns: JWT token + user info.
        /// Store the token in Redux state and send it in every future request.
        /// </remarks>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            // 200 OK — no new resource was created, just authentication
            return Ok(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // GET /api/auth/me
        // ════════════════════════════════════════════════════════════════════
        /// <summary>
        /// Get the currently logged-in user's profile.
        /// Requires: Authorization: Bearer {token}
        /// </summary>
        [HttpGet("me")]
        [Authorize]   // ← This means: JWT token REQUIRED — 401 if missing
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMe()
        {
            // Extract the user's ID from their JWT token
            // "User" here is the ClaimsPrincipal — built into ControllerBase
            var userId = _jwtService.GetUserIdFromToken(User);

            var profile = await _authService.GetProfileAsync(userId);
            return Ok(profile);
        }

        // ════════════════════════════════════════════════════════════════════
        // PUT /api/auth/profile
        // ════════════════════════════════════════════════════════════════════
        /// <summary>Update your name and phone number.</summary>
        [HttpPut("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var result = await _authService.UpdateProfileAsync(userId, dto);
            return Ok(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // PUT /api/auth/change-password
        // ════════════════════════════════════════════════════════════════════
        /// <summary>Change your password. Must provide current password to confirm.</summary>
        [HttpPut("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            await _authService.ChangePasswordAsync(userId, dto);

            // Return a simple success message
            return Ok(new { message = "Password changed successfully" });
        }
    }
}