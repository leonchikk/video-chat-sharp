using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public InputAudioDevice()
        {
            Setup();
        }

        public IEnumerable<AudioDeviceCapability> DeviceCapabilities
        {
            get
            {
                int waveInDevices = WaveIn.DeviceCount;
                for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
                {
                    WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                    yield return new AudioDeviceCapability(waveInDevice, deviceInfo.Channels, deviceInfo.ProductName);
                }
            }
        }

        public event Action<AudioSampleRecordedEventArgs> OnSampleRecorded;

        //TODO: Rename AudioDeviceCapability
        public void SwitchTo(AudioDeviceCapability capability)
        {
            //TODO: Add error event here
            if (capability == null)
            {
                return;
            }

            _audioRecorder = new WaveIn();
            _audioRecorder.DataAvailable += RecorderOnDataAvailable;
            _audioRecorder.WaveFormat = new WaveFormat(48000, 1);
            _audioRecorder.DeviceNumber = capability.DeviceNumber;
            _audioRecorder.BufferMilliseconds = 1000 / 30;

            _isSetuped = true;
        }

        private void RecorderOnDataAvailable(object sender, WaveInEventArgs e)
        {
            OnSampleRecorded?.Invoke(new AudioSampleRecordedEventArgs(e.Buffer, e.BytesRecorded));
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
            if (!DeviceCapabilities.Any())
            {
                //TODO: Add error event here
                return;
            }

            SwitchTo(DeviceCapabilities.First());
        }
    }
}
