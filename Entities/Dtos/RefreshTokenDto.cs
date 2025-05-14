namespace Entities.Dtos
{
    public class RefreshTokenDto
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; } = false;
        public string JwtId { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public UserDto User { get; set; } = default!;
    }
}
