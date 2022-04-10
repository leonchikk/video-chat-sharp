using System;
using System.Collections.Generic;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia
{
    public interface IInputAudioDevice : IDisposable
    {
        event Action<AudioSampleRecordedEventArgs> OnSamplesRecorded;

        AudioDeviceOptions SelectedOption { get; }
        IEnumerable<AudioDeviceOptions> Options { get; }
        void SwitchTo(AudioDeviceOptions options);
        void Start();
        void Stop();
    }
}
