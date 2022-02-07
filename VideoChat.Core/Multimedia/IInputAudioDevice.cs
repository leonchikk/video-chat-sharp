﻿using System;
using System.Collections.Generic;
using VideoChat.Core.Models;

namespace VideoChat.Core.Multimedia
{
    public interface IInputAudioDevice : IDisposable
    {
        event Action<AudioSampleRecordedEventArgs> OnSampleRecorded;

        IEnumerable<AudioDeviceCapability> DeviceCapabilities { get; }

        void SwitchTo(AudioDeviceCapability capability);

        void Start();
        void Stop();
    }
}
