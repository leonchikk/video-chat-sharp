namespace VoiceEngine.Abstractions.Models
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

        public override bool Equals(object obj)
        {
            var info = (VideoDeviceInfo)obj;

            return info.MonikerString == this.MonikerString && info.Name == this.Name;
        }
    }
}
