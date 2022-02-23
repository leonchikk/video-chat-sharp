using Multimedia.Audio.Desktop.Codecs;
using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia;

namespace Multimedia.Audio.Desktop
{
    //TODO: Add OnError event
    public class InputAudioDevice : IInputAudioDevice
    {
        private WaveIn _audioRecorder;
        private bool _isSetuped = false;
        private bool _disposed = false;
        private OpusAudioCodec _codec;

        public InputAudioDevice()
        {
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

        //TODO: Rename AudioDeviceCapability
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
            _audioRecorder.BufferMilliseconds = 200;

            _isSetuped = true;
        }

        private void RecorderOnDataAvailable(object sender, WaveInEventArgs e)
        {
            short[] buffer = new short[e.BytesRecorded];
            Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesRecorded);

            if (buffer.All(x => IsSilence(x, 45)))
                return;

            var samples = _codec.Encode(e.Buffer, 0, e.BytesRecorded);

            foreach (var sample in samples)
            {
                OnSampleRecorded?.Invoke(new AudioSampleRecordedEventArgs(sample, sample.Length));
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

            _codec = new OpusAudioCodec();

            SwitchTo(Options.First());
        }

        private bool IsSilence(float amplitude, sbyte threshold)
        {
            double dB = 20 * Math.Log10(Math.Abs(amplitude));
            return dB < threshold;
        }
    }
}
