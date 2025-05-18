namespace Client.WPF.Services.Interfaces
{
    public interface IUserStore
    {
        event Action<Entities.Dtos.UserDto?, bool>? OnUserSignChanged;
        Entities.Dtos.UserDto? CurrentUser { get; }

        void SetUser(Entities.Dtos.UserDto? user);
        string? StoredUsername { get; set; }
        Entities.Dtos.ConversationDto? StoredConversation { get; set; }
    }
}
