using MulTUNG;
using MulTUNG.Packeting.Packets;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG
{
    public static class Network
    {
        public static void ProcessPacket(Packet packet, int playerId)
        {
            if (packet is PlayerWelcomePacket wel)
            {
                IGConsole.Log("Your player ID is " + wel.YourID);

                MulTUNG.NetClient.SetID(wel.YourID);
            }
            if (packet.SenderID == playerId)
            {
                return;
            }

            switch (packet)
            {
                case PlayerStatePacket state:
                    if (state.PlayerID != playerId)
                    {
                        PlayerManager.UpdatePlayer(state);
                    }

                    break;
                case PlaceBoardPacket board:
                    if (board.AuthorID != playerId)
                    {
                        NetUtilitiesComponent.Instance.Enqueue(new PlaceBoardJob(board));
                    }

                    break;
                case DeleteBoardPacket del:
                    NetUtilitiesComponent.Instance.Enqueue(new DeleteBoardJob(del.BoardID));

                    break;
                case PlaceComponentPacket placeComp:
                    NetUtilitiesComponent.Instance.Enqueue(new PlaceComponentJob(placeComp));

                    break;
                case DeleteComponentPacket delComp:
                    NetUtilitiesComponent.Instance.Enqueue(new DeleteComponentJob(delComp.ComponentNetID));

                    break;
                case CircuitUpdatePacket imnotgonnausethis:
                    MyFixedUpdate.Instance?.ForceUpdate();

                    break;
            }
        }
    }
}
