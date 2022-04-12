﻿using System.IO;
using VideoChat.Core.Packets;

namespace Networking.Factories
{
    public static class PacketFactory
    {
        public static AudioPacket ToAudioPacket(this byte[] payload)
        {
            using (var memoryStream = new MemoryStream(payload))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    var containsSpeech = binaryReader.ReadBoolean();
                    var samplesCount = binaryReader.ReadInt32();
                    var samples = binaryReader.ReadBytes(samplesCount);

                    return new AudioPacket(containsSpeech, samples);
                }
            }
        }
    }
}