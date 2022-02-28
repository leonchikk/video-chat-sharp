using FragLabs.Audio.Codecs;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Multimedia.Audio.Desktop.Codecs
{
    public class OpusAudioCodec
    {
        private OpusEncoder _encoder;
        private OpusDecoder _decoder;
        private int _segmentFrames;
        private int _bytesPerSegment;
        private byte[] _notEncodedBuffer = new byte[0];

        public OpusAudioCodec()
        {
            _segmentFrames = 2880;
            _encoder = OpusEncoder.Create(48000, 1, FragLabs.Audio.Codecs.Opus.Application.Audio);
            _encoder.Bitrate = 56000;
            _decoder = OpusDecoder.Create(48000, 1);
            _bytesPerSegment = _encoder.FrameByteCount(_segmentFrames);
        }

        public IEnumerable<byte[]> Encode(byte[] data, int offset, int length)
        {
            byte[] soundBuffer = new byte[length + _notEncodedBuffer.Length];
            for (int i = 0; i < _notEncodedBuffer.Length; i++)
                soundBuffer[i] = _notEncodedBuffer[i];
            for (int i = 0; i < length; i++)
                soundBuffer[i + _notEncodedBuffer.Length] = data[i];

            int byteCap = _bytesPerSegment;
            int segmentCount = (int)Math.Floor((decimal)soundBuffer.Length / byteCap);
            int segmentsEnd = segmentCount * byteCap;
            int notEncodedCount = soundBuffer.Length - segmentsEnd;
            _notEncodedBuffer = new byte[notEncodedCount];
            for (int i = 0; i < notEncodedCount; i++)
            {
                _notEncodedBuffer[i] = soundBuffer[segmentsEnd + i];
            }

            for (int i = 0; i < segmentCount; i++)
            {
                byte[] segment = new byte[byteCap];
                for (int j = 0; j < segment.Length; j++)
                    segment[j] = soundBuffer[(i * byteCap) + j];
                int len;

                byte[] buff = _encoder.Encode(segment, segment.Length, out len);

                yield return buff;
            }
        }

        public byte[] Decode(byte[] data, int length)
        {
            return _decoder.Decode(data, length, out _);
        }
    }
}
