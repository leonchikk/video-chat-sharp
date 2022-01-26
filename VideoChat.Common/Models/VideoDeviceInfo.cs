namespace VideoChat.Core.Models
{
    public class VideoDeviceInfo
    {
        public VideoDeviceInfo(int index, string name, string moniker)
        {
            Index = index;
            Name = name;
            MonikerString = moniker;
        }

        public int Index { get; set; }
        public string Name { get; set; }
        public string MonikerString { get; set; }
    }
}
