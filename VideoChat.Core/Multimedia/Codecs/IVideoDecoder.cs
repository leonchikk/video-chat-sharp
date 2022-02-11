using System;
using System.Drawing;

namespace VideoChat.Core.Multimedia.Codecs
{
    public interface IVideoDecoder
    {
        event Action<Bitmap> OnDecode;

        void Decode(byte[] stream);
    }
}
