using System.IO;

namespace Multimedia.Video.AVI.RIFF
{
    // https://msdn.microsoft.com/ja-jp/library/cc352264.aspx
    internal class RiffChunk : RiffBase
    {
        private BinaryWriter _writer;
        private BinaryReader _reader;

        public RiffChunk(Stream output, string fourCC) : base(output, fourCC)
        {
            _writer = new BinaryWriter(output);
        }

        public RiffChunk(Stream input) : base(input)
        {
            _reader = new BinaryReader(input);
        }

        public byte[] ReadBytes(int count)
        {
            return _reader.ReadBytes(count);
        }

        public byte[] ReadToEnd()
        {
            var count = ChunkSize - _reader.BaseStream.Position + DataOffset;
            var bytes = ReadBytes((int)count);
            if (count % 2 > 0) _reader.BaseStream.Seek(1, SeekOrigin.Current);
            return bytes;
        }

        public void SkipToEnd()
        {
            var count = ChunkSize - _reader.BaseStream.Position + DataOffset;
            if (count > 0) _reader.BaseStream.Seek(count, SeekOrigin.Current);
            if (count % 2 > 0) _reader.BaseStream.Seek(1, SeekOrigin.Current);
        }

        public void Write(byte[] data)
        {
            _writer.BaseStream.Write(data, 0, data.Length);
        }
        public void Write(int value)
        {
            _writer.Write(value);
        }
        public void WriteByte(byte value)
        {
            _writer.Write(value);
        }
    }
}
