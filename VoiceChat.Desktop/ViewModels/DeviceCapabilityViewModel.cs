using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using VideoChat.Core.Models;

namespace VoiceChat.Desktop.ViewModels
{
    public class DeviceCapabilityViewModel : INotifyPropertyChanged
    {
        public VideoDeviceOptions CurrentCapability { get; set; }
        public ObservableCollection<VideoDeviceOptions> Capabilities { get; set; }

        public DeviceCapabilityViewModel(IEnumerable<VideoDeviceOptions> capabilities, VideoDeviceOptions currentCapability)
        {
            Capabilities = new ObservableCollection<VideoDeviceOptions>(capabilities);
            CurrentCapability = currentCapability == null ? Capabilities.FirstOrDefault() : Capabilities.FirstOrDefault(x => x.DeviceNumber == currentCapability.DeviceNumber);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
