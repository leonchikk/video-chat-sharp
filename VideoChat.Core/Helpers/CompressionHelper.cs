﻿using System.IO;
using System.IO.Compression;

namespace VideoChat.Core.Helpers
{
    public static class CompressionHelper
    {
        public static byte[] Compress(this byte[] data)
        {
            byte[] compressArray = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(data, 0, data.Length);
                }
                compressArray = memoryStream.ToArray();
            }

            return compressArray;
        }

        public static byte[] Decompress(this byte[] data)
        {
            byte[] decompressedArray = null;

            using (MemoryStream decompressedStream = new MemoryStream())
            {
                using (MemoryStream compressStream = new MemoryStream(data))
                {
                    using (DeflateStream deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(decompressedStream);
                    }
                }
                decompressedArray = decompressedStream.ToArray();
            }

            return decompressedArray;
        }
    }
}
