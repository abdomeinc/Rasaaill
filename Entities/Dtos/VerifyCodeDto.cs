using System.ComponentModel.DataAnnotations;

namespace Entities.Dtos
{
    public class VerifyCodeDto
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
