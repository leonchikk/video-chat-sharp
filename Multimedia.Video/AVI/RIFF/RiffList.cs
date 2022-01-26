using System.IO;

namespace Multimedia.Video.AVI.RIFF
{
    // https://msdn.microsoft.com/ja-jp/library/cc352264.aspx
    internal class RiffList : RiffBase
    {
        public string Id { get; private set; }

        private BinaryWriter _writer;

        public RiffList(Stream output, string fourCC, string id) : base(output, fourCC)
        {
            _writer = new BinaryWriter(output);
            _writer.Write(ToFourCC(id));

            Id = id;
        }

        public RiffList(Stream input) : base(input)
        {
            if (input.Length - input.Position < 4)
            {
                Broken = true;
                return;
            }

            var reader = new BinaryReader(input);
            this.Id = ToFourCC(reader.ReadInt32());
        }

        public RiffList CreateList(string fourCC)
        {
            return new RiffList(_writer.BaseStream, "LIST", fourCC);
        }

        public RiffChunk CreateChunk(string fourCC)
        {
            return new RiffChunk(_writer.BaseStream, fourCC);
        }
    }
}
