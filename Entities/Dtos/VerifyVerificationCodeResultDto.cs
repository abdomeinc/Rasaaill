namespace Entities.Dtos
{
    public class VerifyVerificationCodeResultDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
    }
}
