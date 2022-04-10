namespace VideoChat.Core.Models
{
    public class AudioDeviceOptions
    {
        public AudioDeviceOptions(int deviceNumber, int channels, string name)
        {
            DeviceNumber = deviceNumber;
            Channels = channels;
            Name = name;
        }

        public int DeviceNumber { get; private set; }
        public int Channels { get; private set; }
        public string Name { get; private set; }

        public override bool Equals(object obj)
        {
            return (obj as AudioDeviceOptions).DeviceNumber == DeviceNumber;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
