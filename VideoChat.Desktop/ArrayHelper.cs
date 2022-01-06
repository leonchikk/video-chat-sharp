using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Media.Imaging;

namespace VideoChat.Desktop
{
    public static class ArrayHelper
    {
        public static byte[][] BufferSplit(this byte[] buffer, int blockSize)
        {
            byte[][] blocks = new byte[(buffer.Length + blockSize - 1) / blockSize][];

            for (int i = 0, j = 0; i < blocks.Length; i++, j += blockSize)
            {
                blocks[i] = new byte[Math.Min(blockSize, buffer.Length - j)];
                Array.Copy(buffer, j, blocks[i], 0, blocks[i].Length);
            }

            return blocks;
        }

        public static byte[] Compress(this byte[] b)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream z = new GZipStream(ms, CompressionMode.Compress, true))
                    z.Write(b, 0, b.Length);
                return ms.ToArray();
            }
        }
        public static byte[] Decompress(this byte[] b)
        {
            using (var ms = new MemoryStream())
            {
                using (var bs = new MemoryStream(b))
                using (var z = new GZipStream(bs, CompressionMode.Decompress))
                    z.CopyTo(ms);
                return ms.ToArray();
            }
        }

      
    }
}
