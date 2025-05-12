namespace Shared.Services.Interfaces
{
    public interface IMessageService
    {
        Task<Entities.Dtos.MessageSeenInfoDto> MarkMessageAsSeenAsync(Guid messageId, Guid userId);
        Task<Entities.Dtos.MessageDto> SaveMessageAsync(Entities.Dtos.MessageDto message, Guid userId);
    }
}
