using Multimedia.Video.Desktop.AVI.RIFF;
using OpenH264Lib;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoChat.Core.Multimedia.Codecs;

namespace Multimedia.Video.Desktop.Codecs
{
    public class VideoDecoder : IVideoDecoder
    {
        private readonly string _dllName = "openh264-2.1.1-win32.dll";

        public event Action<Bitmap> OnDecode;

        public void Decode(byte[] buffer)
        {
            Task.Run(() =>
            {
                using (var stream = new MemoryStream(buffer))
                {
                    using (var decoder = new Decoder(_dllName))
                    {
                        var riff = new RiffFile(stream);

                        var frames = riff.Chunks.OfType<RiffChunk>().Where(x => x.FourCC == "00dc");

                        foreach (var chunk in frames)
                        {
                            var frame = chunk.ReadToEnd();
                            var decodedImage = decoder.Decode(frame, frame.Length);
                            if (decodedImage == null) continue;

                            OnDecode?.Invoke(decodedImage);
                        }
                    }
                }
            });
        }
    }
}
