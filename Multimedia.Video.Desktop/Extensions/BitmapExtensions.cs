using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Multimedia.Video.Desktop.Extensions
{
    internal static class BitmapExtensions
    {
        public static byte[] ToByteArray(this Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Jpeg);
                return memoryStream.ToArray();
            }
        }
    }
}
