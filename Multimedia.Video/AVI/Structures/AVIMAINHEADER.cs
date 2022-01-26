using System;
using System.Runtime.InteropServices;

namespace Multimedia.Video.AVI.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct AVIMAINHEADER
    {
        public UInt32 dwMicroSecPerFrame;  // only used with AVICOMRPESSF_KEYFRAMES
        public UInt32 dwMaxBytesPerSec;
        public UInt32 dwPaddingGranularity; // only used with AVICOMPRESSF_DATARATE
        public UInt32 dwFlags;
        public UInt32 dwTotalFrames;
        public UInt32 dwInitialFrames;
        public UInt32 dwStreams;
        public UInt32 dwSuggestedBufferSize;
        public UInt32 dwWidth;
        public UInt32 dwHeight;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public UInt32[] dwReserved;
    }
}
