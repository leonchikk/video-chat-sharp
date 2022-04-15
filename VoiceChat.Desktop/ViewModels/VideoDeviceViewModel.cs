using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using VoiceEngine.Abstractions.Models;

namespace VoiceChat.Desktop.ViewModels
{
    public class VideoDeviceViewModel : INotifyPropertyChanged
    {
        public VideoDeviceInfo CurrentDevice { get; set; }
        public ObservableCollection<VideoDeviceInfo> Devices { get; set; }

        public VideoDeviceViewModel(IEnumerable<VideoDeviceInfo> devices, VideoDeviceInfo currentDevice)
        {
            Devices = new ObservableCollection<VideoDeviceInfo>(devices);
            CurrentDevice = Devices.FirstOrDefault(x => x.Name == currentDevice.Name);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
