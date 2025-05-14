using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services.Interfaces
{
    public interface IVerificationCodeStore
    {
        Task StoreCodeAsync(string email, string code, TimeSpan expiresIn);
        Task<string?> GetCodeAsync(string email);
        Task RemoveCodeAsync(string email);
    }
}
