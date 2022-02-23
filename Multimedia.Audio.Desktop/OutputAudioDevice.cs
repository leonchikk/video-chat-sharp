using Multimedia.Audio.Desktop.Codecs;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia;

namespace Multimedia.Audio.Desktop
{
    //TODO: Add OnError event
    public class OutputAudioDevice : IOutputAudioDevice
    {
        private WaveOut _audioPlayer;
        private BufferedWaveProvider _bufferedWaveProvider;
        private bool _isSetuped = false;
        private bool _disposed = false;
        private OpusAudioCodec _codec;

        public OutputAudioDevice()
        {
            _codec = new OpusAudioCodec();

            SwitchTo(Options?.First());
        }

        public IEnumerable<AudioDeviceOptions> Options
        {
            get
            {
                int waveOutDevices = WaveOut.DeviceCount;
                for (int waveOutDevice = 0; waveOutDevice < waveOutDevices; waveOutDevice++)
                {
                    WaveOutCapabilities deviceInfo = WaveOut.GetCapabilities(waveOutDevice);
                    yield return new AudioDeviceOptions(waveOutDevice, deviceInfo.Channels, deviceInfo.ProductName);
                }
            }
        }

        public async Task PlaySamples(byte[] buffer, int desireLatency)
        {
            //TODO: Add error event here
            if (!_isSetuped)
            {
                return;
            }

            //await Task.Delay(desireLatency);

            var decoded = _codec.Decode(buffer, buffer.Length);

            _bufferedWaveProvider?.AddSamples(decoded, 0, decoded.Length);
        }

        public void SwitchTo(AudioDeviceOptions capability)
        {
            //TODO: Add error event here
            if (capability == null)
            {
                return;
            }

            _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 1));
            _audioPlayer = new WaveOut();
            _audioPlayer.DeviceNumber = capability.DeviceNumber;
            _audioPlayer.Init(_bufferedWaveProvider); 
            _isSetuped = true;
        }

        public void Start()
        {
            if (!_isSetuped)
            {
                throw new ApplicationException("Output device is not setuped");
            }

            _audioPlayer.Play();
        }

        public void Stop()
        {
            _audioPlayer?.Stop();
            _bufferedWaveProvider?.ClearBuffer();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _audioPlayer?.Dispose();
        }
    }
}
