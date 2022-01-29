using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia
{
    public interface IAudioDeviceManager : IDisposable
    {
        event Action<AudioSampleRecordedEventArgs> OnSampleRecorded;

        IEnumerable<AudioDeviceCapability> InputDeviceCapabilities { get; }
        IEnumerable<AudioDeviceCapability> OutputDeviceCapabilities { get; }


        void Setup(AudioDeviceCapability inputDeviceCapability, AudioDeviceCapability outputDeviceCapability);
        Task PlaySamples(byte[] buffer, int desireLatency);
        void Start();
        void Stop();
    }
}
