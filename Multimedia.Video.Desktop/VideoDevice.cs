using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia;
using VideoChat.Core.Multimedia.Codecs;

namespace Multimedia.Video.Desktop
{
    public class VideoDevice : IVideoDeviceManager
    {
        public event Action<byte[]> OnCaptureNewFrames;

        private readonly IVideoCodec _codec;

        private IList<VideoDeviceInfo> _captureDevices;
        private VideoCaptureDevice _captureDevice;
        private bool _disposed = false;

        public VideoDevice(IVideoCodec codec)
        {
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _captureDevices = (
                GetDevices()
                    .Select(x => new VideoDeviceInfo(x.Name, x.MonikerString))
             ).ToList();
        }

        public IList<VideoDeviceInfo> AvailableDevices => _captureDevices;

        public IList<VideoDeviceCapability> DeviceCapabilities =>
            _captureDevice?.VideoCapabilities.Select(
                (v, i) => new VideoDeviceCapability()
                {
                    FrameSize = v.FrameSize,
                    DeviceNumber = i,
                    FrameRate = v.AverageFrameRate
                }
             ).ToList();

        public int VideoRecordLatency => _codec.EncodeVideoLatency;

        public void Start(VideoDeviceCapability deviceCapability)
        {
            if (_captureDevice == null)
            {
                throw new NullReferenceException("Capture device is not setuped");
            }

            if (!_codec.IsSetuped)
            {
                throw new ApplicationException("Codec is not setuped");
            }

            if (_captureDevice.IsRunning)
            {
                Stop();
            }

            _captureDevice.VideoResolution = _captureDevice.VideoCapabilities[deviceCapability.DeviceNumber];
            _captureDevice.NewFrame += CaptureNewFrame;
            _captureDevice.Start();
        }

        public void Stop()
        {
            _captureDevice?.Stop();
        }

        private void CaptureNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            _codec.Encode(eventArgs.Frame);
        }

        private void OnCodecEncodeFrames(byte[] buffer)
        {
            OnCaptureNewFrames?.Invoke(buffer);
        }

        public IEnumerable<Bitmap> Decode(byte[] videoBytes)
        {
            return _codec.Decode(videoBytes);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _codec.Dispose();
        }

        private IEnumerable<FilterInfo> GetDevices()
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            for (int i = 0; i < devices.Count; i++)
            {
                yield return devices[i];
            }
        }

        public void SetupDevice(VideoDeviceInfo videoDevice)
        {
            _captureDevice = new VideoCaptureDevice(videoDevice.MonikerString);
        }

        public void SetupCodec(Size size, int bitrate, int fps)
        {
            _codec.OnFramesEncode += OnCodecEncodeFrames;
            _codec.Setup(
               size.Width,
               size.Height,
                bitrate,
                fps
              );
        }
    }
}
