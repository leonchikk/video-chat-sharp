﻿namespace VoiceEngine.Network.Abstractions
{
    public enum PacketTypeEnum : byte
    {
        Video = 0,
        Audio = 1,
        Event = 2,
        UserList = 3,
        FinishHandshake = 4,
        InitHandshake = 5
    }
}
