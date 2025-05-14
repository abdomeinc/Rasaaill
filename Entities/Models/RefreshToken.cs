namespace Entities.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; } = false;
        public string JwtId { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
    }
}
