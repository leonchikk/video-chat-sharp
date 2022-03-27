using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VideoChat.Core.Codec;
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
        private IAudioDecoder _decoder;
        private short[] _pcmBuffer = new short[480];

        public OutputAudioDevice(IAudioDecoder decoder)
        {
            _decoder = decoder;

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

        public void PlaySamples(byte[] buffer, int length, bool containsSpeech = true)
        {
            //TODO: Add error event here
            if (!_isSetuped)
            {
                return;
            }

            var decodedLength = _decoder.Decode(buffer, length, _pcmBuffer);
            var decodedSamples = (MemoryMarshal.Cast<short, byte>(_pcmBuffer)).ToArray();

            _bufferedWaveProvider?.AddSamples(decodedSamples, 0, decodedSamples.Length);
        }

        public void SwitchTo(AudioDeviceOptions capability)
        {
            //TODO: Add error event here
            if (capability == null)
            {
                return;
            }

            _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 1));
            _bufferedWaveProvider.BufferDuration = TimeSpan.FromMilliseconds(500);
            _bufferedWaveProvider.DiscardOnBufferOverflow = true;
            _audioPlayer = new WaveOut();
            _audioPlayer.DeviceNumber = capability.DeviceNumber;
            _audioPlayer.Init(_bufferedWaveProvider);
            _audioPlayer.DesiredLatency = 0;
            _audioPlayer.Volume = 1.0f;
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
