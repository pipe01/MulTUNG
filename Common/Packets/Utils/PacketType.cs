using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Packets
{
    public enum PacketType
    {
        Unknown,
        PlayerWelcome,
        PlayerState,
        PlaceBoard,
        DeleteBoard,
        PlaceComponent,
        DeleteComponent
    }
}
