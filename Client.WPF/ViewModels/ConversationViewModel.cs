using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Models;
using Microsoft.Extensions.Logging;

namespace Client.WPF.ViewModels
{
    public partial class ConversationViewModel : ViewModelBase
    {
        private readonly ILogger<ConversationViewModel> _logger;
        private readonly Services.Interfaces.IUserStore _userStore;

        public event Action<Entities.Dtos.MessageDto>? OnNewMessage;

        [ObservableProperty]
        private Entities.Dtos.ConversationDto? conversation;

        [ObservableProperty]
        private string? messageText;

        [ObservableProperty]
        private bool isTyping;

        public ConversationViewModel(ILogger<ConversationViewModel> logger, Services.Interfaces.IUserStore userStore)
        {
            _logger = logger;
            _userStore = userStore;

            Conversation = _userStore.StoredConversation;
        }



        [RelayCommand]
        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageText)) return;

            if (Conversation is null) return;

            var msg = new Entities.Dtos.MessageDto
            {
                Id = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                Content = MessageText,
                Timestamp = DateTime.UtcNow,
                State = Shared.MessageState.Sending,
                Type = Shared.MessageType.Text,
                ConversationId = Conversation.Id,
                IsSender = true,
            };

            //Conversation.Messages.Add(msg);

            OnNewMessage?.Invoke(msg);

            //await DatabaseService.SaveMessageAsync(msg);
            //await _signalRService.SendMessageAsync(_targetUserId, msg);

            MessageText = string.Empty;

            _logger.LogInformation("Send Message");
        }

        [RelayCommand]
        private async Task AttachFileAsync()
        {
            if (Conversation is null) return;

            // Open file dialog to select a file
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "All Supported Files|*.jpg;*.jpeg;*.png;*.mp4;*.pdf;*.docx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                var msg = new Entities.Dtos.MessageDto
                {
                    Id = Guid.NewGuid(),
                    SenderId = Guid.NewGuid(),
                    Content = MessageText ?? "",
                    Timestamp = DateTime.UtcNow,
                    State = Shared.MessageState.Sending,
                    Type = DetermineFileType(filePath),
                    LocalFilePath = filePath,
                    FilePreview = fileBytes.Take(100).ToArray(), // Preview first 100 bytes
                    FileName = System.IO.Path.GetFileName(filePath),
                    FileSize = fileBytes.Length
                };

                Conversation.Messages.Add(msg);
                //await DatabaseService.SaveMessageAsync(msg); // optionally skip storing file data
                //await _signalRService.SendMessageAsync(_targetUserId, msg);
            }

            _logger.LogInformation("Attach File");
        }

        private Shared.MessageType DetermineFileType(string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath).ToLower();
            if (ext is ".jpg" or ".jpeg" or ".png") return Shared.MessageType.Image;
            if (ext is ".mp4") return Shared.MessageType.Video;
            return Shared.MessageType.Document;
        }

        partial void OnMessageTextChanged(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                IsTyping = false; 
                return; 
            }
            IsTyping = true;
        }
    }
}
