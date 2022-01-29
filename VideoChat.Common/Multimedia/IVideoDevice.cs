using System;
using System.Collections.Generic;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia
{
    public interface IVideoDevice : IDisposable
    {
        event Action<byte[]> OnCaptureNewFrames;
        IEnumerable<byte[]> Decode(byte[] videoBytes);
        IList<VideoDeviceInfo> AvailableDevices { get; }
        void Start(VideoDeviceInfo videoDevice, int width = 640, int height = 320, int bitrate = 1000, int fps = 30);
        void Stop();
    }
}
