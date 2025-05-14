namespace Entities.Models
{
    public class EmailVerification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }
}
