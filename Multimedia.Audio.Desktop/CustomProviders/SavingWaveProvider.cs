using NAudio.Wave;
using System;
using System.IO;

namespace Multimedia.Audio.Desktop.CustomProviders
{
    public class SavingWaveProvider : IWaveProvider, IDisposable
    {
        private readonly IWaveProvider _sourceWaveProvider;
        private readonly WaveFileWriter _writer;
        private bool _isWriterDisposed;

        public SavingWaveProvider(IWaveProvider sourceWaveProvider, Stream stream)
        {
            _sourceWaveProvider = sourceWaveProvider;
            _writer = new WaveFileWriter(stream, _sourceWaveProvider.WaveFormat);
        }

        public WaveFormat WaveFormat => _sourceWaveProvider.WaveFormat;

        public void Dispose()
        {
            if (!_isWriterDisposed)
            {
                _isWriterDisposed = true;
                _writer.Dispose();
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var read = _sourceWaveProvider.Read(buffer, offset, count);

            if (count > 0 && !_isWriterDisposed)
            {
                _writer.Write(buffer, offset, count);
            }
            if (count == 0)
            {
                Dispose();
            }

            return read;
        }
    }
}
