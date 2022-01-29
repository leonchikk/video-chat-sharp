namespace VideoChat.Core.Models
{
    public class VideoDeviceInfo
    {
        public VideoDeviceInfo(string name, string moniker)
        {
            Name = name;
            MonikerString = moniker;
        }

        public string Name { get; set; }
        public string MonikerString { get; set; }
    }
}
