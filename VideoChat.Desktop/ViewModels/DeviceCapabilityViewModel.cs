using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using VideoChat.Core.Models;

namespace VideoChat.Desktop.ViewModels
{
    public class DeviceCapabilityViewModel : INotifyPropertyChanged
    {
        public VideoDeviceCapability CurrentCapability { get; set; }
        public ObservableCollection<VideoDeviceCapability> Capabilities { get; set; }

        public DeviceCapabilityViewModel(IEnumerable<VideoDeviceCapability> capabilities, VideoDeviceCapability currentCapability)
        {
            Capabilities = new ObservableCollection<VideoDeviceCapability>(capabilities);
            CurrentCapability = currentCapability;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
