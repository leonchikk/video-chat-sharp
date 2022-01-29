using System;
using System.Collections.Generic;
using System.Drawing;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia
{
    public interface IVideoDeviceManager : IDisposable
    {
        event Action<byte[]> OnCaptureNewFrames;
        
        IList<VideoDeviceInfo> AvailableDevices { get; }
        IList<VideoDeviceCapability> DeviceCapabilities { get; }
        int VideoRecordLatency { get; }

        IEnumerable<Bitmap> Decode(byte[] videoBytes);
        void SetupDevice(VideoDeviceInfo videoDevice);
        void SetupCodec(Size size, int bitrate, int fps);
        void Start(VideoDeviceCapability deviceCapability);
        void Stop();
    }
}
