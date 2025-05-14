using System.ComponentModel.DataAnnotations;

namespace Entities.Dtos
{
    public class EmailDto
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
