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
        private IEnumerable<VideoDeviceInfo> _captureDevices;
        private IEnumerable<VideoDeviceCapability> _capabilities;
        private VideoCaptureDevice _captureDevice;

        public event Action<Bitmap> OnFrame;

        public IEnumerable<VideoDeviceInfo> AvailableDevices => _captureDevices;
        public IEnumerable<VideoDeviceCapability> DeviceCapabilities => _capabilities;


        private VideoDeviceCapability _currentDeviceCapability;
        public VideoDeviceCapability CurrentDeviceCapability => _currentDeviceCapability;

        private VideoDeviceInfo _currentDeviceInfo;
        public VideoDeviceInfo CurrentDeviceInfo => _currentDeviceInfo;


        public VideoDevice()
        {
            _captureDevices = (
                GetDevices()
                    .Select(x => new VideoDeviceInfo(x.Name, x.MonikerString))
             ).ToList();

            Setup();
        }

        public void SetCapability(VideoDeviceCapability capability)
        {
            if (_captureDevice == null)
            {
                return;
            }

            if (capability == null)
            {
                throw new ArgumentNullException(nameof(capability));
            }

            _captureDevice.VideoResolution = _captureDevice.VideoCapabilities[capability.DeviceNumber];
            _currentDeviceCapability = capability;
        }

        public void SwitchTo(VideoDeviceInfo device)
        {
            if (_captureDevice == null)
            {
                return;
            }

            if (_captureDevice.IsRunning)
            {
                Stop();
            }

            _captureDevice = new VideoCaptureDevice(device.MonikerString);
            _currentDeviceInfo = device;

            Start();
        }

        private void CaptureNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            OnFrame.Invoke(eventArgs.Frame);
        }

        private IEnumerable<FilterInfo> GetDevices()
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            for (int i = 0; i < devices.Count; i++)
            {
                yield return devices[i];
            }
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
            _captureDevice?.Stop();
        }

        private void Setup()
        {
            if (!AvailableDevices.Any())
            {
                return;
            }

            var device = AvailableDevices.Last();

            _currentDeviceInfo = device;
            _captureDevice = new VideoCaptureDevice(device.MonikerString);
            _capabilities = _captureDevice?.VideoCapabilities.Select(
                (v, i) => new VideoDeviceCapability()
                {
                    FrameSize = v.FrameSize,
                    DeviceNumber = i,
                    FrameRate = v.AverageFrameRate
                }
             );

            if (!DeviceCapabilities.Any())
            {
                return;
            }

            //var index = DeviceCapabilities.Where(x => x.FrameSize.Width == DeviceCapabilities.Min(d => d.FrameSize.Width)).First();

            SetCapability(DeviceCapabilities.First());
            //SetCapability(index);
        }
    }
}