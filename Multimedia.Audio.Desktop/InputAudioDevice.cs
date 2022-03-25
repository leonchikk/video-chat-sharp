using Multimedia.Audio.Desktop.Codecs;
using NAudio.Wave;
using RNNoiseWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VideoChat.Core.Codec;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia;
using WebRtcVadSharp;

namespace Multimedia.Audio.Desktop
{
    //TODO: Add OnError event
    public class InputAudioDevice : IInputAudioDevice
    {
        private WaveIn _audioRecorder;
        private bool _isSetuped = false;
        private bool _disposed = false;
        private IAudioEncoder _encoder;
        private INoiseReducer _noiseReducer;

        private const int BufferSize = 1024;
        private readonly byte[] _buffer = new byte[BufferSize];

        public InputAudioDevice(INoiseReducer noiseReducer, IAudioEncoder encoder)
        {
            _encoder = encoder;
            _noiseReducer = noiseReducer;

            Setup();
        }

        public IEnumerable<AudioDeviceOptions> Options
        {
            get
            {
                int waveInDevices = WaveIn.DeviceCount;
                for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
                {
                    WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                    yield return new AudioDeviceOptions(waveInDevice, deviceInfo.Channels, deviceInfo.ProductName);
                }
            }
        }

        public event Action<AudioSampleRecordedEventArgs> OnSampleRecorded;

        public void SwitchTo(AudioDeviceOptions capability)
        {
            //TODO: Add error event here
            if (capability == null)
            {
                return;
            }

            _audioRecorder = new WaveIn(WaveCallbackInfo.FunctionCallback());
            _audioRecorder.DataAvailable += RecorderOnDataAvailable;
            _audioRecorder.WaveFormat = new WaveFormat(48000, 16, 1);
            _audioRecorder.DeviceNumber = capability.DeviceNumber;
            _audioRecorder.BufferMilliseconds = 30;

            _isSetuped = true;
        }

        private void RecorderOnDataAvailable(object sender, WaveInEventArgs e)
        {
            var toDenoise = MemoryMarshal.Cast<byte, short>(e.Buffer);

            for (int i = 0; i < toDenoise.Length; i += 480)
            {
                var splittedSample = new short[480];
                Array.Copy(toDenoise.ToArray(), i, splittedSample, 0, 480);

                _noiseReducer.ReduceNoise(splittedSample, 0);

                var encodedLength = _encoder.Encode(splittedSample, _buffer);
                var encoded = new byte[encodedLength];
                Array.Copy(_buffer, encoded, encodedLength);

                OnSampleRecorded?.Invoke(new AudioSampleRecordedEventArgs(encoded, encoded.Length, true));
            }
        }

        public void Start()
        {
            if (!_isSetuped)
            {
                return;
            }

            _audioRecorder.StartRecording();
        }

        public void Stop()
        {
            _audioRecorder.StopRecording();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _audioRecorder?.Dispose();
        }

        private void Setup()
        {
            if (!Options.Any())
            {
                //TODO: Add error event here
                return;
            }

            SwitchTo(Options.First());
        }

        private bool IsSilence(float amplitude, sbyte threshold)
        {
            double dB = 20 * Math.Log10(Math.Abs(amplitude));
            return dB < threshold;
        }

        private bool DoesFrameContainSpeech(byte[] audioFrame)
        {
            using var vad = new WebRtcVad();
            vad.OperatingMode = OperatingMode.VeryAggressive;
            return vad.HasSpeech(audioFrame, WebRtcVadSharp.SampleRate.Is48kHz, FrameLength.Is30ms);
        }
    }
}
