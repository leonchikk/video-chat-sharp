using Multimedia.Audio.Desktop.Codecs;
using NAudio.Wave;
using RNNoiseWrapper;
using SpeexPreprocessor;
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
        private Preprocessor _preprocessor;

        public InputAudioDevice()
        {
            _preprocessor = new Preprocessor(480, 48000);

            _preprocessor.Denoise = true;
            _preprocessor.Dereverb = true;
            _preprocessor.Agc = false;
            _preprocessor.AgcLevel = 8000;
            _preprocessor.AgcMaxGain = 20;
            _preprocessor.AgcIncrement = 5;

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

        private AudioDeviceOptions _selectedOption;
        public AudioDeviceOptions SelectedOption => _selectedOption;

        public event Action<AudioSampleRecordedEventArgs> OnSamplesRecorded;

        public void SwitchTo(AudioDeviceOptions capability)
        {
            //TODO: Add error event here
            if (capability == null)
            {
                return;
            }

            _audioRecorder = new WaveIn();
            _audioRecorder.DataAvailable += RecorderOnDataAvailable;
            _audioRecorder.WaveFormat = new WaveFormat(48000, 16, 1);
            _audioRecorder.DeviceNumber = capability.DeviceNumber;
            _audioRecorder.BufferMilliseconds = 10;
            _isSetuped = true;

            _selectedOption = capability;
        }

        private void RecorderOnDataAvailable(object sender, WaveInEventArgs e)
        {
            OnSamplesRecorded?.Invoke(new AudioSampleRecordedEventArgs(e.Buffer, e.BytesRecorded, true));
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
