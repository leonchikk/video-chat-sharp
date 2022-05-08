using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using VoiceEngine.Abstractions.IO;

namespace VoiceEngine.IO.Desktop
{
    public class AudioRecorder : IAudioRecorder
    {
        private bool _disposed = false;
        private string _filePath;

        ///private WaveFileWriter _writer = null;
        private MixingSampleProvider _mixingSampleProvider;
        private Dictionary<string, MemoryStream> _mixerProviders;
        public bool IsRecording { get; private set; }

        public AudioRecorder()
        {
            _mixingSampleProvider = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(48000, 1));
            _mixingSampleProvider.ReadFully = true;
            _mixerProviders = new Dictionary<string, MemoryStream>();
        }

        public void AddSamples(string inputId, short[] samples, int length)
        {
            if (!IsRecording)
                return;

            var buffer = MemoryMarshal.Cast<short, byte>(samples).ToArray();
            _mixerProviders[inputId]?.Write(buffer, 0, length);
        }

        public void Start(string filePath)
        {
            if (IsRecording)
            {
                throw new Exception("Recording is running");
            }

            IsRecording = true;

            _filePath = filePath;
            //_writer = new WaveFileWriter(filePath, new WaveFormat(48000, 16, 1));
        }

        public void Stop()
        {
            IsRecording = false;

            WaveFileWriter.CreateWaveFile16(_filePath, _mixingSampleProvider);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            //_writer.Close();
            //_writer?.Dispose();
            //_writer = null;
        }

        public void AddInput(string inputId)
        {
            if (_mixerProviders.ContainsKey(inputId))
                return;

            var memoryStream = new MemoryStream();
            var rawWaveStream = new RawSourceWaveStream(memoryStream, new WaveFormat(48000, 16, 1));

            _mixerProviders.Add(inputId, memoryStream);
            _mixingSampleProvider.AddMixerInput(rawWaveStream);
        }

        public void RemoveInput(string inputId)
        {
            //_mixingSampleProvider.RemoveMixerInput(_mixerProviders[inputId].ToSampleProvider());
            _mixerProviders[inputId].Close();
            _mixerProviders.Remove(inputId);
        }
    }
}
