using System;
using System.Collections.Generic;
using VoiceEngine.Abstractions.Models;

namespace VoiceEngine.Abstractions.IO
{
    public interface IOutputAudioDevice : IDisposable
    {
        AudioDeviceOptions SelectedOption { get; }
        IEnumerable<AudioDeviceOptions> Options { get; }
        void SwitchTo(AudioDeviceOptions options);
        void ChangeVolume(float volume);
        void PlaySamples(short[] pcmBuffer, int length);
        void Start();
        void Stop();
    }
}
