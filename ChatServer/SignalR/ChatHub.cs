using Microsoft.AspNetCore.SignalR;

namespace ChatServer.SignalR
{
    public class ChatHub : Hub
    {
        private readonly Services.Interfaces.IFirebaseService _firebase;
        private readonly Services.Interfaces.IDiscoveryService _discovery;

        public ChatHub(Services.Interfaces.IFirebaseService firebase, Services.Interfaces.IDiscoveryService discovery)
        {
            _firebase = firebase;
            _discovery = discovery;
        }

        public override async Task OnConnectedAsync()
        {
            var ip = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
            _discovery.RegisterClient(Context.ConnectionId, ip ?? "0.0.0.0");
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(Shared.Models.Message message)
        {
            // Validate company domain
            if (!message.SenderEmail.EndsWith("@yourcompany.com"))
                throw new HubException("Invalid sender domain");

            // Store in Firebase
            await _firebase.StoreMessageAsync(message);

            if (message.GroupId is not null)
            {
                Guid groupId = Guid.Parse(message.GroupId.ToString() ?? "");
                // Broadcast to recipients
                var targets = _discovery.GetRecipients(groupId);
                await Clients.Clients(targets).SendAsync("ReceiveMessage", message);
            }
            else if (message.ReceiverId is not null)
            {
                string connectionId = message.ReceiverId.ToString() ?? "";
                // TODO:
                // We need to get the actual user connection Id
                await Clients.Clients(connectionId).SendAsync("ReceiveMessage", message);
            }
        }

        public async Task JoinGroup(Guid groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
            _discovery.AddToGroup(Context.ConnectionId, groupId);
        }

    }
}
