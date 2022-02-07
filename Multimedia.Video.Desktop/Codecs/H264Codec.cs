﻿using Multimedia.Video.Desktop.AVI;
using Multimedia.Video.Desktop.AVI.RIFF;
using OpenH264Lib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia.Codecs;
using static OpenH264Lib.Encoder;

namespace Multimedia.Video.Desktop.Codecs
{
    public class H264Codec : IVideoCodec
    {
        private const string _dllName = "openh264-2.1.1-win32.dll";
        private const int _requiredFramesAmountToEncode = 3;

        private readonly Encoder _encoder;
        private readonly Decoder _decoder;
        private readonly MemoryStream _videoStream;

        private int _fps;
        private AviWriter _aviWriter;
        private OnEncodeCallback _onEncode;
        private OnEncodeFinishCallback _onEncodeFinish;
        private int _capturedFramesCount = 0;
        private bool _disposed = false;
        private bool _isSetuped = false;

        public int EncodeVideoLatency => _requiredFramesAmountToEncode * (1000 / _fps);

        public bool IsSetuped => _isSetuped;

        public event Action<byte[]> OnFramesEncode;
        public event Action<byte[]> OnEncode;

        public H264Codec()
        {
            _encoder = new Encoder(_dllName);
            _decoder = new Decoder(_dllName);
            _videoStream = new MemoryStream();
            
            _onEncode = (data, length, frameType) =>
            {
                var keyFrame = (frameType == FrameType.IDR) || (frameType == FrameType.I);
                _aviWriter.AddImage(data, keyFrame);
            };

            _onEncodeFinish = () =>
            {
                if (_capturedFramesCount <= _requiredFramesAmountToEncode)
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

                    OnFramesEncode?.Invoke(outputStream.ToArray());
                };
            };
        }

        public void Setup(int width = 640, int height = 320, int bitrate = 1000, int fps = 30)
        {
            _fps = fps;

            _aviWriter = new AviWriter(_videoStream, "H264", width, height, _fps); //TODO: Consider some way to put here configuration
            _encoder.Setup(width, height, 1000 * bitrate, _fps, 0.1f, _onEncode, _onEncodeFinish); // 0.02f - 60 fps, 0.04, 00.8 - 30fps

            _isSetuped = true;
        }

        public void Setup(VideoDeviceCapability capability)
        {
            if (capability == null)
            {
                throw new ArgumentNullException(nameof(capability));
            }

            Setup(capability.FrameSize.Width, capability.FrameSize.Height, 1000, capability.FrameRate);
        }

        public IEnumerable<Bitmap> Decode(byte[] stream)
        {
            var decoder = new Decoder(_dllName);

            using (var outputStream = new MemoryStream(stream))
            {
                var riff = new RiffFile(outputStream);

                var chunks = riff.Chunks.OfType<RiffChunk>().Where(x => x.FourCC == "00dc");

                foreach (var chunk in chunks)
                {
                    //Thread.Sleep(1000 / _fps);

                    var frame = chunk.ReadToEnd();
                    var image = decoder.Decode(frame, frame.Length);
                    if (image == null) continue;

                    yield return image;
                }
            }
        }

        public void Encode(Bitmap bitmap)
        {
            _capturedFramesCount++;
            _encoder?.Encode(bitmap);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _encoder.Dispose();
            _decoder.Dispose();
        }
    }
}
