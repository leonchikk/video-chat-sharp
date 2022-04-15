using System;
using System.Collections.Generic;
using VoiceEngine.Abstractions.EventArgs;
using VoiceEngine.Abstractions.Models;

namespace VoiceEngine.Abstractions.Multimedia
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
