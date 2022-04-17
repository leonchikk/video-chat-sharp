﻿using System.IO;
using VoiceEngine.Network.Abstractions.Packets;

namespace VoiceEngine.Network.Abstractions.Extensions
{
    public static class AudioPacketExtension
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