using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Server.SignalR.Controllers
{
    /// <summary>
    /// Controller responsible for handling authentication-related operations such as requesting verification codes,
    /// verifying codes, refreshing login tokens, and validating authentication tokens.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Logger instance for logging authentication events and errors.
        /// </summary>
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// UserManager instance for managing user-related operations.
        /// </summary>
        private readonly UserManager<Entities.Models.User> _userManager;

        /// <summary>
        /// Application configuration instance for accessing configuration settings.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Application database context for accessing and manipulating database entities.
        /// </summary>
        private readonly Entities.ApplicationDbContext _dbContext;

        /// <summary>
        /// Verification service for sending and verifying email verification codes.
        /// </summary>
        private readonly Shared.Services.Interfaces.IVerificationService _verificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="userManager">UserManager instance.</param>
        /// <param name="configuration">Configuration instance.</param>
        /// <param name="dbContext">Database context instance.</param>
        /// <param name="verificationService">Verification service instance.</param>
        public AuthController(
            ILogger<AuthController> logger,
            UserManager<Entities.Models.User> userManager,
            IConfiguration configuration,
            Entities.ApplicationDbContext dbContext,
            Shared.Services.Interfaces.IVerificationService verificationService)
        {
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _dbContext = dbContext;
            _verificationService = verificationService;
        }

        /// <summary>
        /// Requests a verification code to be sent to the specified email address.
        /// </summary>
        /// <param name="model">The email DTO containing the email address.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [AllowAnonymous]
        [HttpPost("request-verification-code")]
        public async Task<IActionResult> RequestCode([FromBody] Entities.Dtos.EmailDto model)
        {
            Entities.Models.User? user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("Email not found.");
            }

            var code = await _verificationService.SendVerificationCodeAsync(model.Email);
            // Store code in memory cache with expiration (e.g. 10 mins)
            Entities.Models.EmailVerification verification = new()
            {
                Email = model.Email,
                Code = code,
                Expiry = DateTime.UtcNow.AddMinutes(10)
            };

            _ = _dbContext.EmailVerifications.Add(verification);
            _ = await _dbContext.SaveChangesAsync();

            return Ok("Verification code sent.");
        }

        /// <summary>
        /// Verifies the provided verification code for the specified email address.
        /// </summary>
        /// <param name="model">The verification code DTO containing the email and code.</param>
        /// <returns>An IActionResult containing the JWT token and refresh token if successful.</returns>
        [AllowAnonymous]
        [HttpPost("verify-verification-code")]
        public async Task<IActionResult> VerifyVerificationCode([FromBody] Entities.Dtos.VerifyCodeDto model)
        {
            (bool success, string? error, Entities.Models.User? user) = await _verificationService.VerifyCodeAsync(model.Email, model.Code);
            if (!success || user == null)
            {
                return BadRequest(error);
            }

            (string token, Entities.Dtos.RefreshTokenDto refreshToken) = await GenerateJwtToken(user);

            return Ok(new Entities.Dtos.VerifyVerificationCodeResultDto()
            {
                Token = token,
                RefreshToken = refreshToken.Token,
                Expires = refreshToken.Expires
            });
        }

        /// <summary>
        /// Refreshes the login token using a valid refresh token.
        /// </summary>
        /// <param name="model">The token refresh request DTO containing the access and refresh tokens.</param>
        /// <returns>An IActionResult containing the new access and refresh tokens if successful.</returns>
        [AllowAnonymous]
        [HttpPost("refresh-login-token")]
        public async Task<IActionResult> RefreshLoginToken([FromBody] Entities.Dtos.TokenRefreshRequestDto model)
        {
            ClaimsPrincipal principal = GetPrincipalFromExpiredToken(model.AccessToken);
            Guid userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");
            string? jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);

            Entities.Models.RefreshToken? token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == model.RefreshToken && t.JwtId == jti && !t.IsRevoked && t.Expires > DateTime.UtcNow);

            if (token == null)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }

            Entities.Models.User? user = await _userManager.FindByIdAsync(userId.ToString());
            (string newAccessToken, Entities.Dtos.RefreshTokenDto newRefreshToken) = await GenerateJwtToken(user!);

            // Revoke old token
            token.IsRevoked = true;
            _ = _dbContext.RefreshTokens.Update(token);
            _ = await _dbContext.SaveChangesAsync();

            return Ok(new { Token = newAccessToken, RefreshToken = newRefreshToken.Token });
        }

        /// <summary>
        /// Validates the current authentication token.
        /// </summary>
        /// <returns>An IActionResult indicating the token is valid.</returns>
        [HttpGet("validate")]
        public IActionResult Validate()
        {
            return Ok(new { valid = true });
        }

        /// <summary>
        /// Generates a JWT access token and a refresh token for the specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate the tokens.</param>
        /// <returns>A tuple containing the access token and refresh token DTO.</returns>
        private async Task<(string accessToken, Entities.Dtos.RefreshTokenDto refreshToken)> GenerateJwtToken(Entities.Models.User user)
        {
            string jwtId = Guid.NewGuid().ToString();

            // Initialize a list of claims
            List<Claim> claims = new()
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.DisplayName),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? ""),
                    // Add Custom Claims
                    new Claim("avatar_url", (user.AvatarUrl ?? "").ToLower()),
                    new Claim("is_approved", user.IsApproved.ToString().ToLower())
                };

            // Add User Roles
            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            foreach (string role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add other custom claims stored in the database
            IList<Claim> userDbClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userDbClaims);

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? ""));

            // Check key size BEFORE creating SigningCredentials
            if (key.KeySize < 128) // HS256 requires minimum 128 bits (16 bytes)
            {
                _logger.LogError("JWT Secret key size is {KeySize} bits, but HS256 requires at least 128 bits.", key.KeySize);
                throw new InvalidOperationException($"JWT Secret key size is {key.KeySize} bits, but HS256 requires at least 128 bits. Update your configuration.");
            }

            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            Entities.Models.RefreshToken refreshToken = new()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                JwtId = jwtId,
                UserId = user.Id
            };

            // Store refresh token in DB
            _ = _dbContext.RefreshTokens.Add(refreshToken);
            _ = await _dbContext.SaveChangesAsync();

            return (accessToken, new Entities.Dtos.RefreshTokenDto() { Token = refreshToken.Token, Expires = refreshToken.Expires, JwtId = refreshToken.JwtId, UserId = refreshToken.UserId });
        }

        /// <summary>
        /// Retrieves the claims principal from an expired JWT token.
        /// </summary>
        /// <param name="token">The expired JWT token.</param>
        /// <returns>The claims principal extracted from the token.</returns>
        /// <exception cref="SecurityTokenException">Thrown if the token is invalid.</exception>
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // Allow expired tokens
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = key
            };

            JwtSecurityTokenHandler handler = new();
            ClaimsPrincipal principal = handler.ValidateToken(token, tokenValidationParameters, out SecurityToken? validatedToken);

            return validatedToken is not JwtSecurityToken jwt || !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256)
                ? throw new SecurityTokenException("Invalid token")
                : principal;
        }
    }
}
