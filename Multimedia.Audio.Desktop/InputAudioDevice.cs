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
            _audioRecorder.BufferMilliseconds = 150;

            _isSetuped = true;
        }

        private void RecorderOnDataAvailable(object sender, WaveInEventArgs e)
        {
            short[] buffer = new short[e.BytesRecorded];
            Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesRecorded);

            if (buffer.All(x => IsSilence(x, 50)))
                return;

            var codec = new G722AudioCodec();
            var encoded = codec.Encode(e.Buffer, e.BytesRecorded);

            OnSampleRecorded?.Invoke(new AudioSampleRecordedEventArgs(encoded, encoded.Length));

            //_writer.Write(e.Buffer, 0, e.BytesRecorded);

            //int oldLength = (int)_audioStream.Length;
            //if (_writer != null)
            //    _writer.Write(e.Buffer, 0, e.BytesRecorded);

            //if (_audioStream.Length > 0)
            //{
            //    int newLength = (int)_audioStream.Length;

            //    if (oldLength != newLength)
            //    {
            //        _audioStream.Seek(0, SeekOrigin.Begin);
            //        byte[] buf = _audioStream.GetBuffer();

            //        //var reader = new Mp3FileReader(_audioStream);
            //        //var waveOut = new WaveOut();
            //        //waveOut.Init(reader);
            //        //waveOut.Play();

            //        _bufferedWaveProvider?.AddSamples(buf, 0, buf.Length);

            //        Array.Clear(buf, 0, buf.Length);

            //        _audioStream.Seek(0, SeekOrigin.Begin);
            //        _audioStream.SetLength(0);
            //        _audioStream.Capacity = 0;

            //        //_bufferedWaveProvider.ClearBuffer();

            //        //_bufferedWaveProvider?.AddSamples(recordedAudioBuffer, 0, recordedAudioBuffer.Length);
            //        //OnSampleRecorded?.Invoke(new AudioSampleRecordedEventArgs(recordedAudioBuffer, recordedAudioBuffer.Length));
            //    }
            //}
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

        private bool IsSilence(float amplitude, sbyte threshold)
        {
            double dB = 20 * Math.Log10(Math.Abs(amplitude));
            return dB < threshold;
        }
    }
}
