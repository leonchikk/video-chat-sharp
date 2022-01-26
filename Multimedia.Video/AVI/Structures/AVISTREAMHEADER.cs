using System;
using System.Runtime.InteropServices;

namespace Multimedia.Video.AVI.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct AVISTREAMHEADER
    {
        public Int32 fccType;
        public Int32 fccHandler;
        public Int32 dwFlags;
        public Int16 wPriority;
        public Int16 wLanguage;
        public Int32 dwInitialFrames;
        public Int32 dwScale;
        public Int32 dwRate;
        public Int32 dwStart;
        public Int32 dwLength;
        public Int32 dwSuggestedBufferSize;
        public Int32 dwQuality;
        public Int32 dwSampleSize;
        public RECT rcFrame;
    }
}
