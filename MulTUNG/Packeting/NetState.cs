namespace Client.Packeting
{
    public class NetState
    {
        public const int BufferSize = 4096;
        public byte[] Buffer { get; set; } = new byte[BufferSize];
    }
}
