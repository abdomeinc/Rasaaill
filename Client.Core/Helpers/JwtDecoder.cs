using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Client.Core.Helpers
{
    public static class JwtDecoder
    {
        public static Entities.Dtos.UserDto? ParseToken(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            var claims = token.Claims.ToList();

            return new Entities.Dtos.UserDto
            {
                Id = Guid.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? ""),
                DisplayName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "",
                EmailAddress = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "",
                AvatarUrl = claims.FirstOrDefault(c => c.Type == "avatar_url")?.Value ?? "",
                IsApproved = bool.Parse(claims.FirstOrDefault(c => c.Type == "is_approved")?.Value ?? "false"),
                Roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList()
            };
        }

    }
}
