using System;
using System.Collections.Generic;
using System.Drawing;

namespace VideoChat.Core.Multimedia.Codecs
{
    public interface IVideoCodec : IDisposable
    {
        event Action<byte[]> OnFramesEncode;

        int EncodeVideoLatency { get; }
        bool IsSetuped { get; }

        void Setup(int width = 640, int height = 320, int bitrate = 1000, int fps = 30);
        void Encode(Bitmap bitmap);
        IEnumerable<Bitmap> Decode(byte[] stream);
    }
}
