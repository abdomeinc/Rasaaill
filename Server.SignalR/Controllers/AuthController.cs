using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public AuthController(ILogger<AuthController> logger, UserManager<Entities.Models.User> userManager, SignInManager<Entities.Models.User> signInManager, IConfiguration configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("request-code")]
        public async Task<IActionResult> RequestCode([FromBody] Entities.Dtos.RequestCodeDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest("Email not found.");

            var code = new Random().Next(100000, 999999).ToString(); // 6-digit code

            var verification = new Entities.Models.EmailVerification
            {
                Email = dto.Email,
                Code = code,
                Expiry = DateTime.UtcNow.AddMinutes(10)
            };

            _context.EmailVerifications.Add(verification);
            await _context.SaveChangesAsync();

            await _emailSender.SendEmailAsync(dto.Email, "Your Login Code", $"Your code is: {code}");

            return Ok();
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] Entities.Dtos.VerifyCodeDto dto)
        {
            var verification = await _context.EmailVerifications
                .FirstOrDefaultAsync(v => v.Email == dto.Email && v.Code == dto.Code && v.Expiry > DateTime.UtcNow);

            if (verification == null)
                return Unauthorized("Invalid or expired code.");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized("User not found.");

            var (token, refreshToken) = await GenerateJwtToken(user);
            return Ok(new Entities.Dtos.JwtTokenDto { Token = token });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] Entities.Dtos.TokenRefreshRequestDto model)
        {
            var principal = GetPrincipalFromExpiredToken(model.AccessToken);
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");
            var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);

            var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t =>
                t.Token == model.RefreshToken && t.JwtId == jti && !t.IsRevoked && t.Expires > DateTime.UtcNow);

            if (token == null)
                return Unauthorized("Invalid or expired refresh token.");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            var (newAccessToken, newRefreshToken) = await GenerateJwtToken(user!);

            // Revoke old token
            token.IsRevoked = true;
            _dbContext.RefreshTokens.Update(token);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Token = newAccessToken, RefreshToken = newRefreshToken.Token });
        }
        [HttpPost("send-code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] Entities.Dtos.EmailDto dto)
        {
            await _verificationService.SendVerificationCodeAsync(dto.Email);
            return Ok("Verification code sent.");
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyCode([FromBody] Entities.Dtos.EmailCodeDto dto)
        {
            var (success, error, user) = await _verificationService.VerifyCodeAsync(dto.Email, dto.Code);
            if (!success || user == null)
                return BadRequest(error);

            var (token, refreshToken) = await _tokenService.GenerateTokens(user); // your existing logic

            return Ok(new
            {
                Token = token,
                RefreshToken = refreshToken.Token,
                Expires = refreshToken.Expires
            });
        }

        [HttpPost("request-code")]
        public async Task<IActionResult> RequestLoginCode([FromBody] Entities.Dtos.EmailDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsApproved)
                return Unauthorized("User not found or not approved.");

            // Generate 6-digit code
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            // Store code in memory cache with expiration (e.g. 10 mins)
            _cache.Set($"login_code_{model.Email.ToLower()}", code, TimeSpan.FromMinutes(10));

            // Send email
            await _emailSender.SendEmailAsync(user.Email!, "Your verification code", $"Your login code is: {code}");

            return Ok("Verification code sent.");
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyLoginCode([FromBody] Entities.Dtos.VerifyCodeDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsApproved)
                return Unauthorized("Invalid user or not approved.");

            var cachedCode = _cache.Get<string>($"login_code_{model.Email.ToLower()}");
            if (cachedCode == null || cachedCode != model.Code)
                return Unauthorized("Invalid or expired verification code.");

            // Remove used code
            _cache.Remove($"login_code_{model.Email.ToLower()}");

            var token = await GenerateJwtToken(user);
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
            var jwtId = Guid.NewGuid().ToString();

            // --- JWT creation (same as before, with "jti" claim) ---

            // Initialize a list of claims
            var claims = new List<Claim>
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
            var userRoles = await _userManager.GetRolesAsync(user); // Example using UserManager

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Example: Fetching and adding other custom claims stored in your database
            var userDbClaims = await _userManager.GetClaimsAsync(user); // Example using UserManager
            claims.AddRange(userDbClaims);


            // --- Rest of your token generation code ---
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? ""));

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


            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims, // Use the enhanced claims list
                expires: DateTime.Now.AddDays(1), // Adjust expiration as needed
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = new Entities.Dtos.RefreshTokenDto
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                JwtId = jwtId,
                UserId = user.Id
            };

            // Store refresh token in DB
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            return (accessToken, refreshToken);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // <--- IMPORTANT: We allow expired tokens
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = key
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwt || !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
