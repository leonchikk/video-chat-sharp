using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using VideoChat.Core.Models;
using VideoChat.Core.Models.EventArgs;
using VideoChat.Core.Multimedia;
using VideoChat.Core.Multimedia.Codecs;

namespace Multimedia.Video
{
    public class VideoDevice : IVideoDevice
    {
        public event Action<byte[]> OnCaptureNewFrames;

        private const int REQUIRED_FRAMES_COUNT_TO_ENCODE = 3;

        private readonly IVideoCodec _codec;
        private UsbCamera _captureDevice;
        private int _capturedFramesCount = 0;
        private bool _disposing = false;

        public VideoDevice(IVideoCodec codec)
        {
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
        }

        public IEnumerable<VideoDeviceInfo> GetAvailableDevices()
        {
            return UsbCamera.FindDevices().ToList();
        }

        public void Start(VideoDeviceInfo videoDevice, Size screenSize, int bitrate = 1000, int fps = 30)
        {
            if (_captureDevice != null)
            {
                Stop();
            }

            _captureDevice = new UsbCamera(videoDevice.Index, screenSize);
            _captureDevice.NewFrame += CaptureNewFrame;
            _codec.OnFramesEncode += OnCodecEncodeFrames;

            _captureDevice.Start();
        }

        public void Stop()
        {
            _captureDevice?.Stop();
        }

        private void CaptureNewFrame(NewFrameEventArgs eventArgs)
        {
            if (REQUIRED_FRAMES_COUNT_TO_ENCODE == _capturedFramesCount)
            {
                _codec.Encode(eventArgs.Frame);
                _capturedFramesCount = 0;
            }

            _capturedFramesCount++;
        }

        private void OnCodecEncodeFrames(byte[] buffer)
        {
            OnCaptureNewFrames?.Invoke(buffer);
        }

        public void Dispose()
        {
            if (_disposing)
                return;

            _codec.Dispose();

            _disposing = true;
        }
    }
}
