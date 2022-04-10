﻿using System;
using System.Collections.Generic;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia
{
    public interface IOutputAudioDevice : IDisposable
    {
        AudioDeviceOptions SelectedOption { get; }
        IEnumerable<AudioDeviceOptions> Options { get; }
        void SwitchTo(AudioDeviceOptions options);
        void ChangeVolume(float volume);
        void PlaySamples(byte[] buffer, int length, bool containsSpeech = true);
        void Start();
        void Stop();
    }
}
