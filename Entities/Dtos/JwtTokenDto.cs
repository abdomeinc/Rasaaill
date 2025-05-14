namespace Entities.Dtos
{
    public class JwtTokenDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public RefreshTokenDto RefreshToken { get; set; } = default!;
    }
}
