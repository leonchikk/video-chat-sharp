namespace VideoChat.Core.Enumerations
{
    public enum PacketTypeEnum : byte
    {
        Video = 0,
        Audio = 1,
        JoinConversation = 2,
        LeaveConversation = 3,
        CreateConversation = 4,
        CloseConversation = 5,
        Online = 6,
        ConnectionInfo = 7
    }
}
