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
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<Entities.Models.User> _userManager;
        private readonly SignInManager<Entities.Models.User> _signInManager;
        private readonly IConfiguration _configuration;

        private readonly Entities.ApplicationDbContext _dbContext;
        private readonly Shared.Services.Interfaces.IEmailSender _emailSender;
        private readonly Shared.Services.Interfaces.IVerificationService _verificationService;
        private readonly IMemoryCache _cache;

        public AuthController(ILogger<AuthController> logger, UserManager<Entities.Models.User> userManager, SignInManager<Entities.Models.User> signInManager, IConfiguration configuration,
            Entities.ApplicationDbContext dbContext,
            Shared.Services.Interfaces.IEmailSender emailSender,
            Shared.Services.Interfaces.IVerificationService verificationService,
            IMemoryCache cache)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;

            _dbContext = dbContext;
            _emailSender = emailSender;
            _verificationService = verificationService;
            _cache = cache;
        }

        [HttpPost("request-code")]
        public async Task<IActionResult> RequestCode([FromBody] Entities.Dtos.RequestCodeDto dto)
        {
            Entities.Models.User? user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return BadRequest("Email not found.");
            }

            string code = new Random().Next(100000, 999999).ToString(); // 6-digit code

            Entities.Models.EmailVerification verification = new()
            {
                Email = dto.Email,
                Code = code,
                Expiry = DateTime.UtcNow.AddMinutes(10)
            };

            _ = _dbContext.EmailVerifications.Add(verification);
            _ = await _dbContext.SaveChangesAsync();

            await _emailSender.SendEmailAsync(dto.Email, "Your Login Code", $"Your code is: {code}");

            return Ok();
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] Entities.Dtos.VerifyCodeDto dto)
        {
            Entities.Models.EmailVerification? verification = await _dbContext.EmailVerifications
                .FirstOrDefaultAsync(v => v.Email == dto.Email && v.Code == dto.Code && v.Expiry > DateTime.UtcNow);

            if (verification == null)
            {
                return Unauthorized("Invalid or expired code.");
            }

            Entities.Models.User? user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            (string token, Entities.Dtos.RefreshTokenDto refreshToken) = await GenerateJwtToken(user);
            return Ok(new Entities.Dtos.JwtTokenDto { Token = token });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] Entities.Dtos.TokenRefreshRequestDto model)
        {
            ClaimsPrincipal principal = GetPrincipalFromExpiredToken(model.AccessToken);
            Guid userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");
            string? jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);

            Entities.Models.RefreshToken? token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t =>
                t.Token == model.RefreshToken && t.JwtId == jti && !t.IsRevoked && t.Expires > DateTime.UtcNow);

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
        [HttpPost("send-code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] Entities.Dtos.EmailDto dto)
        {
            await _verificationService.SendVerificationCodeAsync(dto.Email);
            return Ok("Verification code sent.");
        }

        [HttpPost("verify-login-code2")]
        public async Task<IActionResult> VerifyCode([FromBody] Entities.Dtos.EmailCodeDto dto)
        {
            (bool success, string? error, Entities.Models.User? user) = await _verificationService.VerifyCodeAsync(dto.Email, dto.Code);
            if (!success || user == null)
            {
                return BadRequest(error);
            }

            (string token, Entities.Dtos.RefreshTokenDto refreshToken) = await GenerateJwtToken(user); // your existing logic

            return Ok(new
            {
                Token = token,
                RefreshToken = refreshToken.Token,
                refreshToken.Expires
            });
        }

        [HttpPost("request-login-code")]
        public async Task<IActionResult> RequestLoginCode([FromBody] Entities.Dtos.EmailDto model)
        {
            Entities.Models.User? user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsApproved)
            {
                return Unauthorized("User not found or not approved.");
            }

            // Generate 6-digit code
            string code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            // Store code in memory cache with expiration (e.g. 10 mins)
            _ = _cache.Set($"login_code_{model.Email.ToLower()}", code, TimeSpan.FromMinutes(10));

            // Send email
            await _emailSender.SendEmailAsync(user.Email!, "Your verification code", $"Your login code is: {code}");

            return Ok("Verification code sent.");
        }

        [HttpPost("verify-login-code")]
        public async Task<IActionResult> VerifyLoginCode([FromBody] Entities.Dtos.VerifyCodeDto model)
        {
            Entities.Models.User? user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsApproved)
            {
                return Unauthorized("Invalid user or not approved.");
            }

            string? cachedCode = _cache.Get<string>($"login_code_{model.Email.ToLower()}");
            if (cachedCode == null || cachedCode != model.Code)
            {
                return Unauthorized("Invalid or expired verification code.");
            }

            // Remove used code
            _cache.Remove($"login_code_{model.Email.ToLower()}");

            (string accessToken, Entities.Dtos.RefreshTokenDto refreshToken) token = await GenerateJwtToken(user);
            return Ok(new { Token = token });
        }


        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] Entities.Dtos.RegisterModelDto model)
        //{
        //    if (await _userManager.FindByEmailAsync(model.Email) != null)
        //        return BadRequest("Email is already taken.");

        //    var user = new Entities.Models.User() { UserName = model.Email, Email = model.Email, IsApproved = false };
        //    var result = await _userManager.CreateAsync(user, model.Password);

        //    if (result.Succeeded)
        //    {
        //        // Send email confirmation logic or approval
        //        return Ok("User created, waiting for approval.");
        //    }

        //    return BadRequest(result.Errors);
        //}

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] Entities.Dtos.LoginModelDto model)
        //{
        //    var user = await _userManager.FindByEmailAsync(model.Email);
        //    if (user == null || !user.IsApproved)
        //    {
        //        _logger.LogWarning("Unauthorized login attempt for email: {Email}", model.Email);
        //        return Unauthorized("User not approved or invalid credentials.");
        //    }


        //    var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
        //    if (!result.Succeeded)
        //    {
        //        _logger.LogWarning("Invalid credentials for email: {Email}", model.Email);
        //        return Unauthorized("Invalid credentials.");
        //    }

        //    var token = GenerateJwtToken(user);
        //    return Ok(new { Token = token });
        //}

        private async Task<(string accessToken, Entities.Dtos.RefreshTokenDto refreshToken)> GenerateJwtToken(Entities.Models.User user) // Made async because fetching roles/claims might be async
        {
            string jwtId = Guid.NewGuid().ToString();

            // --- JWT creation (same as before, with "jti" claim) ---

            // Initialize a list of claims
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? ""),
                // --- Add Custom Claims ---
                new Claim("avatar_url", (user.AvatarUrl ?? "").ToLower()),
                new Claim("is_approved", user.IsApproved.ToString().ToLower())
            };

            // --- Add User Roles ---
            // You'll need to fetch the user's roles here.
            // This often involves your Identity/User management system (e.g., UserManager in ASP.NET Core Identity)
            // The exact implementation depends on how you manage roles and the structure of your User object.
            // Assuming you have a way to get roles, for example:
            IList<string> userRoles = await _userManager.GetRolesAsync(user); // Example using UserManager

            foreach (string role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Example: Fetching and adding other custom claims stored in your database
            IList<Claim> userDbClaims = await _userManager.GetClaimsAsync(user); // Example using UserManager
            claims.AddRange(userDbClaims);


            // --- Rest of your token generation code ---
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? ""));

            // IMPORTANT: Check key size BEFORE creating SigningCredentials
            if (key.KeySize < 128) // HS256 requires minimum 128 bits (16 bytes)
            {
                // Log an error or throw a more specific exception if the key is too short
                _logger.LogError("JWT Secret key size is {KeySize} bits, but HS256 requires at least 128 bits.", key.KeySize);
                // As a fallback, you could potentially use a default key or throw
                throw new InvalidOperationException($"JWT Secret key size is {key.KeySize} bits, but HS256 requires at least 128 bits. Update your configuration.");
                // Or, if you used a default key earlier, use it here instead of throwing
                // key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourVeryLongDefaultSecretKeyIfConfigIsMissingOrShort"));
                // creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            }


            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims, // Use the enhanced claims list
                expires: DateTime.Now.AddDays(1), // Adjust expiration as needed
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

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // <--- IMPORTANT: We allow expired tokens
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
