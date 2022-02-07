using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia
{
    public interface IOutputAudioDevice: IDisposable
    {
        IEnumerable<AudioDeviceCapability> DeviceCapabilities { get; }
        void SwitchTo(AudioDeviceCapability capability);
        Task PlaySamples(byte[] buffer, int desireLatency);
        void Start();
        void Stop();
    }
}
