using Microsoft.Extensions.Caching.Memory;

namespace Shared.Services
{
    public class InMemoryVerificationCodeStore:Interfaces.IVerificationCodeStore
    {
        private readonly IMemoryCache _cache;

        public InMemoryVerificationCodeStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task StoreCodeAsync(string email, string code, TimeSpan expiresIn)
        {
            _cache.Set(email.ToLower(), code, expiresIn);
            return Task.CompletedTask;
        }

        public Task<string?> GetCodeAsync(string email)
        {
            _cache.TryGetValue(email.ToLower(), out string? code);
            return Task.FromResult(code);
        }

        public Task RemoveCodeAsync(string email)
        {
            _cache.Remove(email.ToLower());
            return Task.CompletedTask;
        }

    }
}
