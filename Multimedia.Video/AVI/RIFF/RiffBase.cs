using System;
using System.IO;
using System.Text;

namespace Multimedia.Video.AVI.RIFF
{
    // https://msdn.microsoft.com/ja-jp/library/cc352264.aspx
    internal class RiffBase : IDisposable
    {
        public long Offset { get; private set; }
        public long SizeOffset { get; private set; }
        public long DataOffset { get; private set; }
        public uint ChunkSize { get; private set; }
        public string FourCC { get; private set; }
        public bool Broken { get; protected set; }

        internal static int ToFourCC(string fourCC)
        {
            if (fourCC.Length != 4) throw new ArgumentException("fourCCは4文字である必要があります。", "fourCC");
            return ((int)fourCC[3]) << 24 | ((int)fourCC[2]) << 16 | ((int)fourCC[1]) << 8 | ((int)fourCC[0]);
        }

        internal static string ToFourCC(int fourCC)
        {
            var bytes = new byte[4];
            bytes[0] = (byte)(fourCC >> 0 & 0xFF);
            bytes[1] = (byte)(fourCC >> 8 & 0xFF);
            bytes[2] = (byte)(fourCC >> 16 & 0xFF);
            bytes[3] = (byte)(fourCC >> 24 & 0xFF);
            return Encoding.ASCII.GetString(bytes);
        }

        public RiffBase(Stream output, string fourCC)
        {
            this.FourCC = fourCC;
            this.Offset = output.Position;

            var writer = new BinaryWriter(output);
            writer.Write(ToFourCC(fourCC));

            this.SizeOffset = output.Position;

            uint dummy_size = 0; // sizeに0を書いておく。Close時に正しい値を書き直す。
            writer.Write(dummy_size);

            this.DataOffset = output.Position;

            // Close(Dispose)時に呼び出される処理。
            OnClose = () =>
            {
                // sizeを正しい値に変更する
                var position = writer.BaseStream.Position;
                ChunkSize = (uint)(position - DataOffset);
                writer.BaseStream.Position = SizeOffset;
                writer.Write(ChunkSize);
                writer.BaseStream.Position = position;
            };
        }

        public RiffBase(Stream input)
        {
            var reader = new BinaryReader(input);

            // FourCCとChunkSizeが読めるか？
            if (input.Length - input.Position < 8) { Broken = true; return; }
            // FourCCとChunkSizeを読む。
            this.Offset = input.Position;
            FourCC = ToFourCC(reader.ReadInt32());
            this.SizeOffset = input.Position;
            ChunkSize = reader.ReadUInt32();
            this.DataOffset = input.Position;

            if (input.Position + ChunkSize > input.Length) Broken = true;
        }

        private Action OnClose = () => { };
        public virtual void Close() { OnClose(); }

        #region IDisposable Support
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 管理（managed）リソースの破棄処理をここに記述します。 
                Close();
            }
            // 非管理（unmanaged）リソースの破棄処理をここに記述します。
        }

        ~RiffBase() { Dispose(false); }
        #endregion
    }
}
