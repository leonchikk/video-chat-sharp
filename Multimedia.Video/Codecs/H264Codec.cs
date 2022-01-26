using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using VideoChat.Core.Multimedia.Codecs;

namespace Multimedia.Video.Codecs
{
    public class H264Codec : IVideoCodec
    {
        public event Action<byte[]> OnFramesEncode;

        public IEnumerable<Bitmap> Decode(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Encode(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public void Encode(Stream memoryStream)
        {
            throw new NotImplementedException();
        }
    }
}
