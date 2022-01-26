using System;
using System.Collections.Generic;
using System.Drawing;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia
{
    public interface IVideoDevice: IDisposable
    {
        event Action<byte[]> OnCaptureNewFrames;

        IEnumerable<VideoDeviceInfo> GetAvailableDevices();
        void Start(VideoDeviceInfo videoDevice, Size screenSize, int bitrate = 1000, int fps = 30);
        void Stop();
    }
}
