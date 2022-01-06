using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace VideoChat.Desktop
{
    public static class PacketBuilder
    {
        public static byte[] MakePacket(string metadata, byte[] frame)
        {
            var metaData = Encoding.ASCII.GetBytes(metadata);
            var metaDataLength = BitConverter.GetBytes(metaData.Length);

            var frameLength = BitConverter.GetBytes(frame.Length);

            var buffer = new List<byte>();
            buffer.AddRange(metaDataLength);
            buffer.AddRange(metaData);

            buffer.AddRange(frameLength);
            buffer.AddRange(frame);

            return buffer.ToArray();
        }

        public static byte[] MakePacket(string metadata, BitmapImage frame)
        {
            //var metaData = Encoding.ASCII.GetBytes(metadata);
            //var metaDataLength = BitConverter.GetBytes(metaData.Length);

            var frameBytes = ImageToByte(frame);
            //var frameLength = BitConverter.GetBytes(frameBytes.Length);

            //var buffer = new List<byte>();
            //buffer.AddRange(metaDataLength);
            //buffer.AddRange(metaData);

            //buffer.AddRange(frameLength);
            //buffer.AddRange(frameBytes);

            return frameBytes;
        }

        public static byte[] ImageToByte(BitmapImage img)
        {
            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }

            return data;
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            var memory = new MemoryStream();

            bitmap.Save(memory, ImageFormat.Jpeg);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }
}
