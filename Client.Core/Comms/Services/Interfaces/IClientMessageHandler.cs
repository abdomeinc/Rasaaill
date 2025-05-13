namespace Client.Core.Comms.Services.Interfaces
{
    public interface IClientMessageHandler
    {
        Task OnMessageReceived(Entities.Dtos.MessageDto message);
        Task OnTyping(Guid conversationId);
        Task OnMessageSeen(Guid messageId);
        Task OnUpdateTypingStatus(List<Guid> typingUsers);
    }
}
