using Multimedia.Video.Desktop.AVI;
using OpenH264Lib;
using System;
using System.Drawing;
using System.IO;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia.Codecs;
using static OpenH264Lib.Encoder;

namespace Multimedia.Video.Desktop.Codecs
{
    public class VideoEncoder : IVideoEncoder
    {
        private const string _dllName = "openh264-2.1.1-win32.dll";
        private const int _requiredFramesAmountToDencode = 3;

        private Encoder _encoder;
        private MemoryStream _videoStream;

        private int _fps;
        private AviWriter _aviWriter;
        private OnEncodeCallback _onEncode;
        private OnEncodeFinishCallback _onEncodeFinish;
        private int _capturedFramesCount = 0;
        private bool _disposed = false;
        private bool _isSetuped = false;

        public event Action<byte[]> OnEncode;

        public VideoEncoder()
        {
            _onEncode = (data, length, frameType) =>
            {
                var keyFrame = (frameType == FrameType.IDR) || (frameType == FrameType.I);
                _aviWriter.AddImage(data, keyFrame);
            };

            _onEncodeFinish = () =>
            {
                if (_capturedFramesCount <= _requiredFramesAmountToDencode)
                    return;

                _capturedFramesCount = 0;
                _videoStream.Seek(0, SeekOrigin.Begin);

                using (var outputStream = new MemoryStream())
                {
                    _videoStream.CopyTo(outputStream);
                    byte[] buf = _videoStream.GetBuffer();

                    //Reset old buffer, left only service data (AVI headers, which is first 224 bytes)
                    Array.Clear(buf, 224, buf.Length - 224);

                    _videoStream.Seek(0, SeekOrigin.End);
                    _videoStream.SetLength(224);
                    _videoStream.Capacity = 224;

                    outputStream.Position = 0;
                    outputStream.Seek(0, SeekOrigin.Begin);

                    OnEncode?.Invoke(outputStream.ToArray());
                };
            };
        }

        public void Setup(int width = 640, int height = 320, int bitrate = 1000, int fps = 30)
        {
            _fps = fps;

            _aviWriter = new AviWriter(_videoStream, "H264", width, height, _fps); //TODO: Consider some way to put here configuration
            //FRAME RATE INTERVAL AND FPS ARE INDEPENDENT FROM DEVICE FRAME RATE
            _encoder.Setup(width, height, bitrate, _fps, 0.04f, _onEncode, _onEncodeFinish); // 0.02f - 60 fps, 0.04, 00.8 - 30fps

            _isSetuped = true;
        }

        public void Setup(VideoDeviceCapability capability)
        {
            if (capability == null)
            {
                throw new ArgumentNullException(nameof(capability));
            }

            _encoder = new Encoder(_dllName);
            _videoStream = new MemoryStream();

            Setup(capability.FrameSize.Width, capability.FrameSize.Height, 1000 * 1000, 60);
        }

        public void Encode(Bitmap bitmap)
        {
            if (!_isSetuped)
            {
                throw new Exception("Encoder is not setuped");
            }

            _capturedFramesCount++;
            _encoder?.Encode(bitmap);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _encoder.Dispose();
        }
    }
}
