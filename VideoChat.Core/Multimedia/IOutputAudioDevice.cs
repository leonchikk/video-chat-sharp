using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia
{
    public interface IOutputAudioDevice: IDisposable
    {
        IEnumerable<AudioDeviceOptions> Options { get; }
        void SwitchTo(AudioDeviceOptions options);
        void PlaySamples(byte[] buffer);
        void Start();
        void Stop();
    }
}
