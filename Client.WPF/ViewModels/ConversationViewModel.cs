using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.WPF.ViewModels
{
    public class ConversationViewModel : ViewModelBase
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
