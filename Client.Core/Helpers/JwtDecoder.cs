using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Client.Core.Helpers
{
    /// <summary>
    /// Provides helper methods for decoding and parsing JWT tokens.
    /// </summary>
    public static class JwtDecoder
    {
        /// <summary>
        /// Parses a JWT token and extracts user information into a <see cref="Entities.Dtos.UserDto"/> object.
        /// </summary>
        /// <param name="jwt">The JWT token string to parse.</param>
        /// <returns>
        /// A <see cref="Entities.Dtos.UserDto"/> object containing user information extracted from the token claims,
        /// or <c>null</c> if parsing fails.
        /// </returns>
        public static Entities.Dtos.UserDto? ParseToken(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            var claims = token.Claims.ToList();

            return new Entities.Dtos.UserDto
            {
                Id = Guid.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? ""),
                DisplayName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "",
                PhoneNumber = claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value ?? "",
                EmailAddress = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "",
                AvatarUrl = claims.FirstOrDefault(c => c.Type == "avatar_url")?.Value ?? "",
                IsApproved = bool.Parse(claims.FirstOrDefault(c => c.Type == "is_approved")?.Value ?? "false"),
                Roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList()
            };
        }

        public static DateTime? GetExpiration(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            var expClaim = token.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (expClaim == null) return null;

            if (long.TryParse(expClaim, out long secondsSinceEpoch))
            {
                // 'exp' is in UNIX timestamp format
                var expiration = DateTimeOffset.FromUnixTimeSeconds(secondsSinceEpoch).UtcDateTime;
                return expiration;
            }

            return null;
        }
    }
}
