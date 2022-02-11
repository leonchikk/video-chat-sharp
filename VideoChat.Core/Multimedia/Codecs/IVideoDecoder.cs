using System;
using System.Drawing;

namespace VideoChat.Core.Multimedia.Codecs
{
    public interface IVideoDecoder
    {
        void Decode(byte[] stream, Action<Bitmap> displayBitmapAction);
    }
}
