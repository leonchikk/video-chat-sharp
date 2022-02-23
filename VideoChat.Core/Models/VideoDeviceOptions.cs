using System.Drawing;

namespace VideoChat.Core.Models
{
    public class VideoDeviceOptions
    {
        public int FrameRate { get; set; }
        public int DeviceNumber { get; set; }
        public Size FrameSize { get; set; }

        public string FriendlyName => $"{FrameSize.Width} x {FrameSize.Height}, {FrameRate} FPS";

        public override bool Equals(object obj)
        {
            var info = (VideoDeviceOptions)obj;

            return info.DeviceNumber == this.DeviceNumber;
        }
    }
}
