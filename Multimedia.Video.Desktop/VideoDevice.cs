using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia;

namespace Multimedia.Video.Desktop
{
    public class VideoDevice : IVideoDevice
    {
        private IEnumerable<VideoDeviceOptions> _options;
        private VideoCaptureDevice _captureDevice;

        public event Action<Bitmap> OnFrame;

        public IEnumerable<VideoDeviceOptions> DeviceOptions => _options;


        private VideoDeviceOptions _currentDeviceOption;
        public VideoDeviceOptions CurrentOption => _currentDeviceOption;

        private VideoDeviceInfo _info;
        public VideoDeviceInfo Info => _info;

        public VideoDevice(VideoDeviceInfo device)
        {
            Setup(device);
        }

        public void SetOption(VideoDeviceOptions option)
        {
            if (_captureDevice == null)
            {
                return;
            }

            if (option == null)
            {
                return;
            }

            _captureDevice.VideoResolution = _captureDevice.VideoCapabilities[option.DeviceNumber];
            _currentDeviceOption = option;
        }

        public void SwitchTo(VideoDeviceInfo device)
        {
            if (device == null)
            {
                return;
            }

            if (_captureDevice != null && _captureDevice.IsRunning)
            {
                Stop();
            }

            _captureDevice = new VideoCaptureDevice(device.MonikerString);
            _info = device;
            _options = GetCapabilities();

            SetOption(_options.First());
        }

        private void CaptureNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            OnFrame?.Invoke(eventArgs.Frame);
        }

        private IEnumerable<VideoDeviceOptions> GetCapabilities()
        {
            return _captureDevice?.VideoCapabilities.Select(
               (v, i) => new VideoDeviceOptions()
               {
                   FrameSize = v.FrameSize,
                   DeviceNumber = i,
                   FrameRate = v.AverageFrameRate
               }
            ); 
        }

        public void Start()
        {
            if (_captureDevice == null)
            {
                return;
            }

            if (_captureDevice.IsRunning)
            {
                return;
            }

            _captureDevice.NewFrame += CaptureNewFrame;
            _captureDevice.Start();
        }

        public void Stop()
        {
            _captureDevice.NewFrame -= CaptureNewFrame;
            _captureDevice?.Stop();
        }

        private void Setup(VideoDeviceInfo device)
        {
            _captureDevice = new VideoCaptureDevice(device.MonikerString);
            _options = GetCapabilities();

            if (!DeviceOptions.Any())
            {
                return;
            }

            var capability = DeviceOptions.Last();
            SetOption(capability);
        }
    }
}