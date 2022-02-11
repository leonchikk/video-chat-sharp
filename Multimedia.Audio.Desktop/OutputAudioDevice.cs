﻿using NAudio.Wave;
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

        public OutputAudioDevice()
        {
            SwitchTo(DeviceCapabilities?.First());
        }

        public IEnumerable<AudioDeviceCapability> DeviceCapabilities
        {
            get
            {
                int waveOutDevices = WaveOut.DeviceCount;
                for (int waveOutDevice = 0; waveOutDevice < waveOutDevices; waveOutDevice++)
                {
                    WaveOutCapabilities deviceInfo = WaveOut.GetCapabilities(waveOutDevice);
                    yield return new AudioDeviceCapability(waveOutDevice, deviceInfo.Channels, deviceInfo.ProductName);
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

            await Task.Delay(desireLatency);

            _bufferedWaveProvider?.AddSamples(buffer, 0, buffer.Length);
        }

        public void SwitchTo(AudioDeviceCapability capability)
        {
            //TODO: Add error event here
            if (capability == null)
            {
                return;
            }

            _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 1));
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