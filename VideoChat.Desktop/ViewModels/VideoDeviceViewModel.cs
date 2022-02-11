using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using VideoChat.Core.Models;

namespace VideoChat.Desktop.ViewModels
{
    public class VideoDeviceViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<VideoDeviceInfo> Devices { get; private set; }

        public VideoDeviceViewModel(IEnumerable<VideoDeviceInfo> devices)
        {
            Devices = new ObservableCollection<VideoDeviceInfo>(devices);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
