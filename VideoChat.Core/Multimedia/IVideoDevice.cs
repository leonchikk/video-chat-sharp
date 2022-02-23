using System;
using System.Collections.Generic;
using System.Drawing;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia
{
    public interface IVideoDevice
    {
        event Action<Bitmap> OnFrame;

        IEnumerable<VideoDeviceOptions> DeviceOptions { get; }
        VideoDeviceOptions CurrentOption { get; }
        VideoDeviceInfo Info { get; }

        void SwitchTo(VideoDeviceInfo device);
        void SetOption(VideoDeviceOptions capability);

        void Start();
        void Stop();
    }
}
