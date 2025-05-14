namespace Entities.Dtos
{
    public class TokenRefreshRequestDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;

        /*
            When the client sends a token refresh request, it sends two tokens:

            The old expired AccessToken

            The RefreshToken string that was stored securely

            Your server then validates the RefreshToken and checks if it matches and is not revoked. A bool can never fulfill this check.
         */
    }
}
