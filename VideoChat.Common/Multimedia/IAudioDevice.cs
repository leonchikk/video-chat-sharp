using System;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia
{
    public interface IAudioDevice : IDisposable
    {
        event Func<AudioSampleRecordedEventArgs> OnSampleRecorded;
        void AddRecordSamples(byte[] buffer, int offset, int count);
        void Start(int kbps);
        void Stop();
    }
}
