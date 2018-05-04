namespace MulTUNG.Packeting.Packets
{
    public enum PacketType
    {
        Unknown,
        PlayerWelcome,
        PlayerState,
        PlayerDisconnect,
        PlaceBoard,
        DeleteBoard,
        PlaceComponent,
        DeleteComponent,
        CircuitUpdate,
        PlaceWire,
        DeleteWire,
        RotateComponent
    }
}
