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
        IEnumerable<VideoDeviceOptions> DeviceCapabilities { get; }

        VideoDeviceOptions CurrentDeviceCapability { get; }
        VideoDeviceInfo CurrentDeviceInfo { get; }

        void SwitchTo(VideoDeviceInfo device);
        void SetCapability(VideoDeviceOptions capability);

        void Start();
        void Stop();
    }
}
