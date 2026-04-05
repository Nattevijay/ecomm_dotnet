using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Models;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Helpers
{
    // ════════════════════════════════════════════════════════════════════════
    // JWT SERVICE
    // ════════════════════════════════════════════════════════════════════════
    // JWT = JSON Web Token
    // A JWT is a string that proves who the user is without hitting the DB.
    //
    // Structure:  header.payload.signature
    //   header  : algorithm info (HS256)
    //   payload : userId, email, role, expiry — readable by anyone
    //   signature: secret key — only YOUR server can create/verify this
    //
    // FLOW:
    //   1. User logs in → AuthService calls GenerateToken(user)
    //   2. Token returned to React → stored in Redux + localStorage
    //   3. React sends token in every request header:
    //      Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
    //   4. .NET validates it automatically using the secret key
    //
    // REGISTER in Program.cs:
    //   builder.Services.AddScoped<JwtService>();
    // ════════════════════════════════════════════════════════════════════════

    public class JwtService
    {
        private readonly IConfiguration _config;

        // Read all JWT settings from appsettings.json
        private string SecretKey      => _config["Jwt:Key"]!;
        private string Issuer         => _config["Jwt:Issuer"]!;
        private string Audience       => _config["Jwt:Audience"]!;
        private int    ExpiryMinutes  => int.Parse(_config["Jwt:ExpiryMinutes"]!);

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        // ── Generate Token ───────────────────────────────────────────────
        /// <summary>
        /// Creates a signed JWT token containing the user's Id, Email, and Role.
        /// Called right after successful login or registration.
        /// </summary>
        public string GenerateToken(User user)
        {
            // Claims = the data we embed inside the token (readable by React too)
            var claims = new[]
            {
                // Standard claims
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Name,           user.Name),
                new Claim(ClaimTypes.Role,           user.Role),

                // Custom claims (extra data we want to include)
                new Claim("userId", user.Id.ToString()),
                new Claim("role",   user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // unique token id
            };

            // Sign the token with our secret key
            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Build the token
            var token = new JwtSecurityToken(
                issuer:             Issuer,
                audience:           Audience,
                claims:             claims,
                notBefore:          DateTime.UtcNow,
                expires:            DateTime.UtcNow.AddMinutes(ExpiryMinutes),
                signingCredentials: creds
            );

            // Convert token object → string ("eyJhbGci...")
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ── Get User ID from Token ────────────────────────────────────────
        /// <summary>
        /// Extracts the logged-in user's ID from the HttpContext.
        /// Use this inside Controllers to know WHO is making the request.
        ///
        /// Usage in Controller:
        ///   int userId = _jwtService.GetUserIdFromToken(User);
        /// </summary>
        public int GetUserIdFromToken(ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
                     ?? principal.FindFirst("userId");

            if (claim == null)
                throw new UnauthorizedAccessException("User ID not found in token");

            return int.Parse(claim.Value);
        }

        // ── Get Role from Token ───────────────────────────────────────────
        /// <summary>
        /// Extracts the logged-in user's role from the HttpContext.
        /// </summary>
        public string GetRoleFromToken(ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Role)?.Value
                ?? principal.FindFirst("role")?.Value
                ?? string.Empty;
        }
    }
}