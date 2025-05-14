using System.Security.Cryptography;
using System.Text;

namespace Client.Core.Comms.Services
{
    public class TokenStorageService : Interfaces.ITokenStorageService
    {
        private const string TokenPath = "user.token";

        private static readonly string TokenFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rasaaill",
            "jwt.token"
        );

        public void Save(string jwt)
        {
            //File.WriteAllText(TokenPath, jwt);

            Directory.CreateDirectory(Path.GetDirectoryName(TokenFilePath)!);

            var encrypted = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(jwt),
                null,
                DataProtectionScope.CurrentUser
            );

            File.WriteAllBytes(TokenFilePath, encrypted);
        }

        public string? Load()
        {
            //return File.Exists(TokenPath) ? File.ReadAllText(TokenPath) : null;

            if (!File.Exists(TokenFilePath))
                return null;

            try
            {
                var encrypted = File.ReadAllBytes(TokenFilePath);
                var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                return null;
            }
        }

        public void Clear()
        {
            if (File.Exists(TokenFilePath))
            {
                File.Delete(TokenFilePath);
            }
        }
    }
}
