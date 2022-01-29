using Multimedia.Audio.Desktop.CustomProviders;
using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia;

namespace Multimedia.Audio.Desktop
{
    public class AudioDevice : IAudioDeviceManager
    {
        public event Action<AudioSampleRecordedEventArgs> OnSampleRecorded;

        private WaveIn _audioRecorder;
        private BufferedWaveProvider _bufferedWaveProvider;
        private SavingWaveProvider _savingWaveProvider;
        private WaveOut _audioPlayer;
        private MemoryStream _audioMemoryStream;
        private bool _disposed = false;

        public IEnumerable<AudioDeviceCapability> InputDeviceCapabilities
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

        public IEnumerable<AudioDeviceCapability> OutputDeviceCapabilities
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

        public AudioDevice()
        {
            _audioMemoryStream = new MemoryStream();
        }

        public async Task PlaySamples(byte[] buffer, int desireLatency)
        {
            await Task.Delay(desireLatency);

            _bufferedWaveProvider?.AddSamples(buffer, 0, buffer.Length);
        }

        public void Start()
        {
            if (_audioRecorder is null || _audioPlayer is null)
            {
                throw new ApplicationException("Audio has not been setuped yet");
            }

            _audioRecorder.StartRecording();
            _audioPlayer.Play();
        }

        public void Stop()
        {
            _audioRecorder?.StopRecording();
            _audioPlayer?.Stop();
        }

        private void RecorderOnDataAvailable(object sender, WaveInEventArgs e)
        {
            OnSampleRecorded?.Invoke(new AudioSampleRecordedEventArgs(e.Buffer, e.BytesRecorded));
        }

        public void Setup(AudioDeviceCapability inputDeviceCapability, AudioDeviceCapability outputDeviceCapability)
        {
            if (inputDeviceCapability is null)
            {
                throw new ArgumentNullException("Input device is null");
            }

            if (outputDeviceCapability is null)
            {
                throw new ArgumentNullException("Output device is null");
            }

            _audioRecorder = new WaveIn();
            _audioRecorder.DataAvailable += RecorderOnDataAvailable;
            _audioRecorder.WaveFormat = new WaveFormat(16000, 1);
            _audioRecorder.DeviceNumber = inputDeviceCapability.DeviceNumber;

            _bufferedWaveProvider = new BufferedWaveProvider(_audioRecorder.WaveFormat);
            _savingWaveProvider = new SavingWaveProvider(_bufferedWaveProvider, _audioMemoryStream);

            _audioPlayer = new WaveOut();
            _audioPlayer.DeviceNumber = outputDeviceCapability.DeviceNumber;
            _audioPlayer.Init(_savingWaveProvider);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _audioPlayer?.Dispose();
            _audioPlayer?.Dispose();
        }
    }
}
