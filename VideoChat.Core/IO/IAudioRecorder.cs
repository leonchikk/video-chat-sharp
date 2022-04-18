﻿using System;

namespace VoiceEngine.Abstractions.IO
{
    public interface IAudioRecorder: IDisposable
    {
        public bool IsRecording { get; }

        public void AddSamples(byte[] samples);
        public void Start(string filePath);
        public void Stop();
    }
}