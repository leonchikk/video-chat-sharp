using System;
using System.Collections.Generic;
using System.Drawing;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia
{
    public interface IVideoDevice
    {
        event Action<Bitmap> OnFrame;

        IEnumerable<VideoDeviceInfo> AvailableDevices { get; }
        IEnumerable<VideoDeviceCapability> DeviceCapabilities { get; }

        VideoDeviceCapability CurrentDeviceCapability { get; }

        void SwitchTo(VideoDeviceInfo device);
        void SetCapability(VideoDeviceCapability capability);

        void Start();
        void Stop();
    }
}
