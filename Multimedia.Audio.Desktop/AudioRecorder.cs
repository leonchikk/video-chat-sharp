using NAudio.Wave;
using System;
using VoiceEngine.Abstractions.IO;

namespace VoiceEngine.IO.Desktop
{
    public class AudioRecorder : IAudioRecorder
    {
        private bool _disposed = false;

        private WaveFileWriter _writer = null;
        public bool IsRecording { get; private set; }

        public AudioRecorder()
        {

        }

        public void AddSamples(byte[] samples)
        {
            if (_writer == null)
            {
                throw new NullReferenceException(nameof(_writer));
            }

            _writer.Write(samples, 0, samples.Length);
        }

        public void Start(string filePath)
        {
            if (IsRecording)
            {
                throw new Exception("Recording is running");
            }

            IsRecording = true;

            _writer = new WaveFileWriter(filePath, new WaveFormat(48000, 16, 1));
        }

        public void Stop()
        {
            IsRecording = false;
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _writer?.Dispose();
        }
    }
}
