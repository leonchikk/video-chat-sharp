namespace Multimedia.Video.Desktop.AVI.RIFF
{
    internal class Idx1Entry
    {
        public string ChunkId { get; private set; }
        public int Length { get; private set; }
        public bool Padding { get; private set; }
        public bool KeyFrame { get; set; }

        public Idx1Entry(string chunkId, int length, bool padding)
        {
            ChunkId = chunkId;
            Length = length;
            Padding = padding;
        }
    }
}
