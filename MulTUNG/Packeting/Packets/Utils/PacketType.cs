namespace MulTUNG.Packeting.Packets
{
    public enum PacketType
    {
        Unknown,
        Signal,
        PlayerWelcome,
        PlayerState,
        PlayerDisconnect,
        PlaceBoard,
        DeleteBoard,
        PlaceComponent,
        DeleteComponent,
        PlaceWire,
        DeleteWire,
        RotateComponent,
    }
}
