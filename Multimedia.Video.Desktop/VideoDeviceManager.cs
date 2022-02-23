using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Linq;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia;

namespace Multimedia.Video.Desktop
{
    public class VideoDeviceManager : IVideoDeviceManager
    {
        private IEnumerable<VideoDeviceInfo> _captureDevices;
        public IEnumerable<VideoDeviceInfo> AvailableDevices => _captureDevices;


        private IVideoDevice _videoDevice;
        public IVideoDevice CurrentDevice => _videoDevice;


        public VideoDeviceManager()
        {
            _captureDevices = (
                GetDevices()
                    .Select(x => new VideoDeviceInfo(x.Name, x.MonikerString))
             ).ToList();

            Setup();
        }

        public void SwitchTo(VideoDeviceInfo device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            _videoDevice = new VideoDevice(_captureDevices.First());
        }

        private void Setup()
        {
            if (!_captureDevices.Any())
                return;

            SwitchTo(_captureDevices.First());
        }

        private IEnumerable<FilterInfo> GetDevices()
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            for (int i = 0; i < devices.Count; i++)
            {
                yield return devices[i];
            }
        }
    }
}
