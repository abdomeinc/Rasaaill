using System.ComponentModel.DataAnnotations;

namespace Entities.Dtos
{
    public class EmailVerificationDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }
}
