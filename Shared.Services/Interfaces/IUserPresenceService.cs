using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services.Interfaces
{
    public interface IUserPresenceService
    {
        void AddConnection(Guid userId, string connectionId);
        void RemoveConnection(string connectionId);
        IEnumerable<string> GetConnectionsForUser(Guid userId);
        Guid GetUserIdByConnectionId(string connectionId);
    }
}
