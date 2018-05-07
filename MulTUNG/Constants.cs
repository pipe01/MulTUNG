namespace MulTUNG
{
    public static class Constants
    {
        public const int Port = 5678;
        public const int PositionUpdateInterval = 1000 / 20;
        public const float MaximumPlayerStateTime = 1;
        public const int PingInterval = 2000;
        public const int PingTimeout = 2000;
        public const int MaxFailedPings = 30000; //TODO Reduce this
    }
}
