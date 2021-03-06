﻿namespace MulTUNG.Packets
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
        UserInput,
        ComponentData,
        CircuitState,
        PlayerData,
        ChatMessage,
        PauseGame,
        PaintBoard
    }
}
