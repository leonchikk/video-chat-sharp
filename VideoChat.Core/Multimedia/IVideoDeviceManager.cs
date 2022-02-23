using System.Collections.Generic;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia
{
    public interface IVideoDeviceManager
    {
        IEnumerable<VideoDeviceInfo> AvailableDevices { get; }
        IVideoDevice CurrentDevice { get; }
        void SwitchTo(VideoDeviceInfo device);
    }
}
