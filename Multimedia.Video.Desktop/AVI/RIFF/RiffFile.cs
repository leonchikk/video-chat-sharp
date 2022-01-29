using System.Collections.Generic;
using System.IO;

namespace Multimedia.Video.Desktop.AVI.RIFF
{
    // https://msdn.microsoft.com/ja-jp/library/cc352264.aspx
    internal class RiffFile : RiffList
    {
        public IEnumerable<RiffBase> Chunks
        {
            get
            {
                var origin = BaseStream.Position; // はじめの場所を覚えておく
                var reader = new BinaryReader(BaseStream);

                while (BaseStream.Position != BaseStream.Length)
                {
                    if (BaseStream.Length - BaseStream.Position < 4) break;   // fourCCを読めるか
                    var fourCC = ToFourCC(reader.ReadInt32());                // fourCCを覗き見
                    reader.BaseStream.Seek(-4, SeekOrigin.Current); // fourCCぶん戻す
                    var item = (fourCC == "LIST") ? new RiffList(BaseStream) : new RiffChunk(BaseStream) as RiffBase;
                    if (item.Broken) break;

                    yield return item;

                    var chunk = item as RiffChunk;                            // chunkのみ
                    if (chunk != null) chunk.SkipToEnd();                     // データの最後に移動。
                }

                BaseStream.Position = origin; // 次回の列挙に備えはじめの場所に戻す
            }
        }

        public Stream BaseStream { get; private set; }

        /// <summary>書き込み用に開く。</summary>
        public RiffFile(Stream output, string fourCC) : base(output, "RIFF", fourCC)
        {
            BaseStream = output;
        }

        /// <summary>読み取り用に開く。</summary>
        public RiffFile(Stream input) : base(input)
        {
            BaseStream = input;
        }

        public override void Close()
        {
            base.Close();
            BaseStream.Close();
        }
    }
}
