namespace Shared
{
    /// <summary>
    /// Specifies the type of conversation.
    /// </summary>
    public enum ConversationType : int
    {
        /// <summary>
        /// A private conversation between two users.
        /// </summary>
        Private,
        /// <summary>
        /// A group conversation with multiple participants.
        /// </summary>
        Group
    }
    /// <summary>
    /// Specifies the filter type for conversations.
    /// </summary>
    public enum ConversationFilterType : int
    {
        /// <summary>
        /// All conversations.
        /// </summary>
        All,
        /// <summary>
        /// Conversations with unread messages.
        /// </summary>
        Unread,
        /// <summary>
        /// Favorite conversations.
        /// </summary>
        Favorites,
        /// <summary>
        /// Conversations with contacts.
        /// </summary>
        Contacts,
        /// <summary>
        /// Conversations with non-contacts.
        /// </summary>
        NonContacts,
        /// <summary>
        /// Group conversations.
        /// </summary>
        Groups,
        /// <summary>
        /// Conversations with drafts.
        /// </summary>
        Drafts
    }
    /// <summary>
    /// Specifies the type of message.
    /// </summary>
    public enum MessageType : int
    {
        /// <summary>
        /// A text message.
        /// </summary>
        Text,
        /// <summary>
        /// An image message.
        /// </summary>
        Image,
        /// <summary>
        /// A video message.
        /// </summary>
        Video,
        /// <summary>
        /// A document message.
        /// </summary>
        Document
    }
    /// <summary>
    /// Specifies the type of notification.
    /// </summary>
    public enum NotificationType : int
    {
        /// <summary>
        /// All notifications.
        /// </summary>
        All,
        /// <summary>
        /// Message notifications only.
        /// </summary>
        Messages,
        /// <summary>
        /// Mute notifications.
        /// </summary>
        Mute
    }

    /// <summary>
    /// Specifies the state of a message.
    /// </summary>
    public enum MessageState : int
    {
        /// <summary>
        /// Unknown state (default fallback).
        /// </summary>
        Unknown = -2,
        /// <summary>
        /// Message failed to send.
        /// </summary>
        Failed,
        /// <summary>
        /// Message was recalled (undo feature).
        /// </summary>
        Recalled,
        /// <summary>
        /// Message was deleted (soft delete, hidden from one side).
        /// </summary>
        Deleted,
        /// <summary>
        /// Message is being sent (local only).
        /// </summary>
        Sending,
        /// <summary>
        /// Message was sent and accepted by the server.
        /// </summary>
        Sent,
        /// <summary>
        /// Message was received by the recipient.
        /// </summary>
        Received,
        /// <summary>
        /// Message was seen (opened) by the recipient.
        /// </summary>
        Seen
    }
}
