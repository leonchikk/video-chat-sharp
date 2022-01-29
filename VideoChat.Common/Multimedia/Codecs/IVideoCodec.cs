using System;
using System.Collections.Generic;

namespace VideoChat.Core.Multimedia.Codecs
{
    public interface IVideoCodec : IDisposable
    {
        event Action<byte[]> OnFramesEncode;

        void Encode(byte[] buffer);
        IEnumerable<byte[]> Decode(byte[] stream);
    }
}
