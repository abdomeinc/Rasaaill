using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChatClient.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly Services.Interfaces.IChatService _chatService;
        private readonly Services.Interfaces.IFileTransferService _fileTransfer;

        [ObservableProperty]
        private string currentMessage = string.Empty;

        [ObservableProperty]
        private Shared.Models.User? currentUser;

        [ObservableProperty]
        private Shared.Models.User? selectedRecipient;

        public ObservableCollection<Shared.Models.Message> Messages { get; } = new();

        public MainViewModel(Services.Interfaces.IChatService chatService, Services.Interfaces.IFileTransferService fileTransfer)
        {
            _chatService = chatService;
            _fileTransfer = fileTransfer;

            _chatService.ConnectAsync("http://localhost:5000");

            _chatService.MessageReceived += (s, msg) =>
                Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
        }

        [RelayCommand]
        public async Task SendMessage()
        {
            if (CurrentUser is null)
                return;

            var message = new Shared.Models.Message
            {
                Content = CurrentMessage,
                SenderEmail = CurrentUser.Email,
                Status = Shared.Enums.MessageStatus.Pending
            };

            await _chatService.SendMessageAsync(message);
            Messages.Add(message);
        }

        [RelayCommand]
        public async Task SendFile()
        {
            if (SelectedRecipient is null)
                return;

            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                await _fileTransfer.SendFileAsync(dialog.FileName, SelectedRecipient.Id);
            }
        }
    }
}
