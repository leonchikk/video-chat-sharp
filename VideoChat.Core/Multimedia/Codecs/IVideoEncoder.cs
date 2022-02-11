using System;
using System.Drawing;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia.Codecs
{
    public interface IVideoEncoder : IDisposable
    {
        event Action<byte[]> OnEncode;

        void Setup(VideoDeviceCapability capability);
        void Setup(int width = 640, int height = 320, int bitrate = 1000, int fps = 30);
        void Encode(Bitmap bitmap);
    }
}
