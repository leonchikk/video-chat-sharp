using Multimedia.Video.Desktop.AVI.RIFF;
using OpenH264Lib;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using VideoChat.Core.Multimedia.Codecs;

namespace Multimedia.Video.Desktop.Codecs
{
    public class VideoDecoder : IVideoDecoder
    {
        private readonly string _dllName = "openh264-2.1.1-win32.dll";

        public void Decode(byte[] buffer, Action<Bitmap> displayBitmapAction)
        {
            Task.Run(() =>
            {
                using (var stream = new MemoryStream(buffer))
                {
                    using (var decoder = new Decoder(_dllName))
                    {
                        var riff = new RiffFile(stream);

                        var frames = riff.Chunks.OfType<RiffChunk>().Where(x => x.FourCC == "00dc");
                        var enumerator = frames.GetEnumerator();
                        var timer = new System.Timers.Timer(1000 / 60) { AutoReset = true };

                        timer.Elapsed += (s, e) =>
                        {
                            if (enumerator.MoveNext() == false)
                            {
                                timer.Enabled = false;
                                timer.Stop();
                                return;
                            }

                            var chunk = enumerator.Current;
                            var frame = chunk.ReadToEnd();
                            var image = decoder.Decode(frame, frame.Length);
                            if (image == null) return;

                            Dispatcher.CurrentDispatcher.BeginInvoke(displayBitmapAction);
                        };
                        timer.Start();
                    }
                }
            });
        }
    }
}
