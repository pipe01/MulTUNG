﻿namespace Client.Packeting.Packets
{
    public enum PacketType
    {
        Unknown,
        PlayerWelcome,
        PlayerState,
        PlaceBoard,
        DeleteBoard,
        PlaceComponent,
        DeleteComponent,
        CircuitUpdate
    }
}