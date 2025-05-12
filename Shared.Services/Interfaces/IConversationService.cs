namespace Shared.Services.Interfaces
{
    public interface IConversationService
    {
        Task<IEnumerable<Guid>> GetParticipants(Guid conversationId);
        Task<IEnumerable<Guid>> GetParticipantUserIdsAsync(Guid conversationId);
    }
}
