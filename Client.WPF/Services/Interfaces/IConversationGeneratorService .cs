namespace Client.WPF.Services.Interfaces
{
    public interface IConversationGeneratorService
    {
        List<Entities.Dtos.ConversationDto> GenerateConversations(int privateCount = 1, int groupCount = 1);
    }

}
