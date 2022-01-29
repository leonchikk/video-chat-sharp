using System;
using System.Runtime.InteropServices;

namespace Multimedia.Video.Desktop.AVI.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct RECT
    {
        public Int16 left;
        public Int16 top;
        public Int16 right;
        public Int16 bottom;
    }
}
