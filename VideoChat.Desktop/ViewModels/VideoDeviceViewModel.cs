﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using VideoChat.Core.Models;

namespace VideoChat.Desktop.ViewModels
{
    public class VideoDeviceViewModel : INotifyPropertyChanged
    {
        public VideoDeviceInfo CurrentDevice { get; set; }
        public ObservableCollection<VideoDeviceInfo> Devices { get; set; }

        public VideoDeviceViewModel(IEnumerable<VideoDeviceInfo> devices, VideoDeviceInfo currentDevice)
        {
            Devices = new ObservableCollection<VideoDeviceInfo>(devices);
            CurrentDevice = currentDevice;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
