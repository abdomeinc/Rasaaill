using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services.Interfaces
{
    public interface IFileTransferService
    {
        Task SendFileAsync(string filePath, string peerId);
        void StartListening();
    }
}
