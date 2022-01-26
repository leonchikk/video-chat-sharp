using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace VideoChat.Core.Multimedia.Codecs
{
    public interface IVideoCodec: IDisposable
    {
        event Action<byte[]> OnFramesEncode;

        void Encode(byte[] buffer);
        void Encode(Stream memoryStream);

        IEnumerable<Bitmap> Decode(Stream stream);
    }
}
