using Microsoft.Extensions.Caching.Memory;

namespace Shared.Services
{
    /// <summary>
    /// Provides an in-memory implementation of <see cref="Interfaces.IVerificationCodeStore"/>
    /// for storing, retrieving, and removing verification codes associated with email addresses.
    /// </summary>
    public class InMemoryVerificationCodeStore : Interfaces.IVerificationCodeStore
    {
        /// <summary>
        /// The in-memory cache used to store verification codes.
        /// </summary>
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryVerificationCodeStore"/> class.
        /// </summary>
        /// <param name="cache">The memory cache instance to use for storing codes.</param>
        public InMemoryVerificationCodeStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Stores a verification code for the specified email address with an expiration time.
        /// </summary>
        /// <param name="email">The email address to associate with the code.</param>
        /// <param name="code">The verification code to store.</param>
        /// <param name="expiresIn">The duration after which the code expires.</param>
        /// <returns>A completed task.</returns>
        public Task StoreCodeAsync(string email, string code, TimeSpan expiresIn)
        {
            _cache.Set($"login_code_{email.ToLower()}", code, expiresIn);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves the verification code associated with the specified email address.
        /// </summary>
        /// <param name="email">The email address whose code to retrieve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the code if found; otherwise, null.
        /// </returns>
        public Task<string?> GetCodeAsync(string email)
        {
            _cache.TryGetValue($"login_code_{email.ToLower()}", out string? code);
            return Task.FromResult(code);
        }

        /// <summary>
        /// Removes the verification code associated with the specified email address.
        /// </summary>
        /// <param name="email">The email address whose code to remove.</param>
        /// <returns>A completed task.</returns>
        public Task RemoveCodeAsync(string email)
        {
            _cache.Remove($"login_code_{email.ToLower()}");
            return Task.CompletedTask;
        }
    }
}
