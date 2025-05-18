using Entities.Dtos;
using Shared;
using System.Text;

namespace Mica.Services
{
    public class ConversationGeneratorService : Interfaces.IConversationGeneratorService
    {
        private readonly Random _random;

        public ConversationGeneratorService()
        {
            _random = new Random();
        }
        public List<ConversationDto> GenerateConversations(int privateCount = 1, int groupCount = 1)
        {
            List<ConversationDto> conversations = new();

            for (int i = 0; i < privateCount; i++)
            {
                conversations.Add(GeneratePrivateConversation(i));
            }

            for (int i = 0; i < groupCount; i++)
            {
                conversations.Add(GenerateGroupConversation(i));
            }

            return conversations;
        }

        private ConversationDto GeneratePrivateConversation(int index)
        {
            UserDto you = CreateUser(true, true, -1);
            UserDto other = CreateUser(false, false, -60 * (index + 1));

            ConversationDto conversation = new()
            {
                Id = Guid.NewGuid(),
                ConversationType = ConversationType.Private,
                NotificationType = NotificationType.All,
                DisplayName = other.DisplayName,
                ImageUrl = other.AvatarUrl,
                CreationDate = DateTime.UtcNow.AddDays(-index - 1),
                Messages = [],
                UnreadCount = new Random().Next(1, 10)
            };

            AddMessages(conversation, new[] { you, other }, 35);
            conversation.LastMessage = conversation.Messages.LastOrDefault();
            conversation.IsSender = conversation.LastMessage?.IsSender ?? false;

            return conversation;
        }

        private void AddMessages(ConversationDto conversation, UserDto[] participants, int count)
        {
            DateTime startTime = DateTime.UtcNow.AddHours(-3);

            for (int i = 0; i < count; i++)
            {
                UserDto sender = participants[_random.Next(participants.Length)];
                bool isSender = sender.DisplayName == "You";

                MessageType type = GetMessageType(i);
                string content = GetContentByType(type, i);
                MessageDto message = new()
                {
                    Id = Guid.NewGuid(),
                    SenderId = Guid.NewGuid(),
                    Sender = sender,
                    Timestamp = startTime.AddSeconds((i * 90) + _random.Next(60)),
                    State = isSender ? MessageState.Seen : MessageState.Received,
                    IsSender = isSender,
                    Type = type,
                    Content = content,
                    Reactions = GenerateReactions(i)
                };

                conversation.Messages.Add(message);
            }
        }

        private MessageType GetMessageType(int index)
        {
            return index % 12 == 5 ? MessageType.Image :
                   index % 12 == 11 ? MessageType.Video :
                   MessageType.Text;
        }

        private string GetContentByType(MessageType type, int index)
        {
            return type switch
            {
                MessageType.Image => $"https://picsum.photos/seed/img{index}/300/200",
                MessageType.Video => $"https://sample-videos.com/video123/mp4/720/big_buck_bunny_720p_{index % 10}.mp4",
                _ => $"Message {index + 1}: {GenerateRandomText(index * 4)}"
            };
        }

        private List<string> GenerateReactions(int index)
        {
            if (index % 10 == 0)
            {
                return ["👍"];
            }

            return index % 15 == 0 ? ["❤️", "😂"] : [];
        }

        private ConversationDto GenerateGroupConversation(int index)
        {
            UserDto you = CreateUser(true, true, -1);
            UserDto lead = CreateUser(false, true, -5);
            UserDto member = CreateUser(false, false, -1440);

            ConversationDto conversation = new()
            {
                Id = Guid.NewGuid(),
                ConversationType = ConversationType.Group,
                NotificationType = NotificationType.Mute,
                DisplayName = GenerateGroupname(),
                ImageUrl = lead.AvatarUrl,
                CreationDate = DateTime.UtcNow.AddDays(-index - 2),
                Messages = [],
                UnreadCount = new Random().Next(1, 10)
            };

            AddMessages(conversation, [you, lead, member], 40);
            conversation.LastMessage = conversation.Messages.LastOrDefault();
            conversation.IsSender = conversation.LastMessage?.IsSender ?? false;

            return conversation;
        }


        private UserDto CreateUser(bool isMe, bool isOnline, int lastSeenMinutesAgo)
        {
            var name = isMe ? "You" : GenerateUsername();
            var email = name.Replace(" ", "").ToLower() + "@misrtech-eg.com";
            return new UserDto
            {
                EmailAddress = email,
                DisplayName = name,
                IsOnline = isOnline,
                LastSeen = DateTime.UtcNow.AddMinutes(lastSeenMinutesAgo),
                AvatarUrl = $"pack://application:,,,/Resources/Views/Splash/spalsh_icon_0{_random.Next(1, 9)}.png"
            };
        }

        private string GenerateUsername()
        {
            int max = UserNames.Length - 1;
            return string.Format("{0} {1}", UserNames[_random.Next(0, max)], UserNames[_random.Next(0, max)]);
        }

        private string GenerateGroupname()
        {
            int max = GroupNames.Length - 1;
            return string.Format("{0}", GroupNames[_random.Next(0, max)]);
        }

        private static readonly string[] SentenceFragments =
        [
            "Hey, how are you doing today?",
            "Did you see the latest update?",
            "Let's catch up later this week.",
            "I’ll send you the report by tomorrow.",
            "That sounds like a great idea!",
            "Can you review the document I sent?",
            "Looking forward to our meeting.",
            "Thanks for your quick response.",
            "What’s your plan for the weekend?",
            "Please let me know if you have questions.",
            "Got it, I'll handle that.",
            "Just finished the task.",
            "Any news on the project?",
            "Let's sync up after lunch.",
            "Good morning! ☀️",
            "I'll be offline for a bit.",
            "Do you need any help?",
            "Sounds perfect to me.",
            "Thanks for the update!",
            "I'll check and get back to you.",
            "Can you send me the link?",
            "Talk soon!",
            "Great job on that last part.",
            "Let me know if anything changes.",
            "Almost done here.",
            "Working on it now.",
            "That’s interesting, tell me more.",
            "I appreciate your help.",
            "Sorry, I missed that message.",
            "Can you clarify that?",
            "See you tomorrow!",
            "No worries, take your time."
        ];

        private static readonly string[] UserNames =
        [
            "Ali",
            "Abdullah",
            "Kamal",
            "Al-Masry",
            "Abdel Aziz",
            "Moyasser",
            "Mohammed",
            "Mostafa",
            "Orfy",
            "Ghaly",
            "Samir",
            "Farag",
            "Islam",
            "Fawzy",
            "Salem",
            "Nasser",
            "Mahmoud",
            "Gamil",
            "Rashad",
            "Karim",
            "Salem",
            "Ghnoneim",
            "Abdel Qader",
            "Abdel Ghany",
            "Walied",
            "Ayman",
            "Abdel Hafiz",
            "El Said",
            "Maged",
            "Magdy",
            "Hagras",
            "Badr"
        ];

        private static readonly string[] GroupNames =
        [
            "🎮 The Geek Squad",
            "👨‍👩‍👧‍👦 Cousins Adda",
            "🎉 Fantastic Four",
            "🦸‍♂️ The Marvels",
            "🚀 Mavericks",
            "👫 The Squad",
            "🏠 Homies",
            "🤝 Through Thick and Thin",
            "🏡 Home Sweet Home",
            "🤜🤛 Bro Gang",
            "🌟 Rising Stars",
            "⚔️ Three Musketeers",
            "🗑️ Recycle Bin",
            "🌪️ Welcome to Chaos",
            "😄 Just Smile, Everyday",
            "📱 Game of Phones",
            "👨‍👩‍👧‍👦 The Family Gang",
            "🤖 Crazy Engineers",
            "🏢 Corporate Life",
            "💰 Future Billionaires",
            "😂 Memers Adda",
            "🤡 Jokers",
            "🤫 Pin Drop Nonsense",
            "🧍‍♂️🧍‍♀️ Strangers",
            "🛡️ The Spartans",
            "🛠️ Engineering in Progress",
            "🧬 Inhumans",
            "🎯 Go-Getters",
            "🗑️ Trash Talk",
            "👔 Mr Perfects"
        ];


        private string GenerateRandomText(int minLength)
        {
            StringBuilder result = new();
            while (result.Length < minLength)
            {
                string sentence = SentenceFragments[_random.Next(SentenceFragments.Length)];
                if (result.Length > 0)
                {
                    _ = result.Append(" ");
                }

                _ = result.Append(sentence);
            }
            return result.ToString().Trim();
        }
    }
}
