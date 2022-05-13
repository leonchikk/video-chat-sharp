using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VoiceEngine.Abstractions.IO;
using VoiceEngine.Abstractions.Models;

namespace VoiceEngine.IO.Desktop
{
    //TODO: Add OnError event
    public class OutputAudioDevice : IOutputAudioDevice
    {
        private WaveOut _audioPlayer;
        private bool _isSetuped = false;
        private bool _disposed = false;
        private MixingSampleProvider _mixingSampleProvider;
        private Dictionary<string, BufferedWaveProvider> _mixerProviders;

        public OutputAudioDevice()
        {
            _mixingSampleProvider = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(48000, 1));
            _mixingSampleProvider.ReadFully = true;
            _mixerProviders = new Dictionary<string, BufferedWaveProvider>();

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

        private AudioDeviceOptions _selectedOption;
        public AudioDeviceOptions SelectedOption => _selectedOption;

        public void PlaySamples(string inputId, short[] pcmBuffer, int length)
        {
            //TODO: Add error event here
            if (!_isSetuped)
            {
                return;
            }

            var buffer = MemoryMarshal.Cast<short, byte>(pcmBuffer).ToArray();

            if (_mixerProviders.ContainsKey(inputId))
                _mixerProviders[inputId]?.AddSamples(buffer, 0, length);
        }

        public void SwitchTo(AudioDeviceOptions capability)
        {
            //TODO: Add error event here
            if (capability == null)
            {
                return;
            }

            _audioPlayer = new WaveOut();
            _audioPlayer.DeviceNumber = capability.DeviceNumber;
            _audioPlayer.Init(_mixingSampleProvider, true);
            _audioPlayer.Volume = 1.0f;
            _audioPlayer.DesiredLatency = 10;
            _isSetuped = true;

            _selectedOption = capability;
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
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _audioPlayer?.Dispose();
        }

        public void ChangeVolume(float volume)
        {
            //TODO: Add error event here
            if (!_isSetuped)
            {
                return;
            }

            _audioPlayer.Volume = volume;
        }

        public void AddInput(string inputId)
        {
            if (_mixerProviders.ContainsKey(inputId))
                return;

            var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 1))
            {
                BufferDuration = TimeSpan.FromMilliseconds(150),
                DiscardOnBufferOverflow = true
            };

            _mixerProviders.Add(inputId, bufferedWaveProvider);
            _mixingSampleProvider.AddMixerInput(bufferedWaveProvider);
        }

        public void RemoveInput(string inputId)
        {
            _mixerProviders[inputId]?.ClearBuffer();
            _mixingSampleProvider.RemoveMixerInput(_mixerProviders[inputId].ToSampleProvider());
            _mixerProviders.Remove(inputId);
        }
    }
}
