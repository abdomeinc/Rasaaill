namespace Shared.Enums
{
    public enum MessageState : int
    {
        Unknown = -2,   // default fallback
        Failed,         // for sending errors
        Recalled,       // for undo message feature
        Deleted,        // soft delete (for hiding from one side)
        Sending,        // just created, local only
        Sent,           // server accepted
        Received,       // receiver got it
        Seen            // receiver opened it
    }
}
