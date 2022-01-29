﻿using System.Drawing;

namespace VideoChat.Core.Models
{
    public class VideoDeviceCapability
    {
        public int FrameRate { get; set; }
        public int DeviceNumber { get; set; }
        public Size FrameSize { get; set; }
    }
}