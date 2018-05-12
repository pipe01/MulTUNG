namespace MulTUNG.Packeting.Packets
{
    public enum PacketType
    {
        Unknown,
        Signal,
        PlayerWelcome,
        PlayerState,
        StateList,
        PlayerDisconnect,
        PlaceBoard,
        DeleteBoard,
        PlaceComponent,
        DeleteComponent,
        PlaceWire,
        DeleteWire,
        RotateComponent,
        WorldData,
        UserInput
    }
}
