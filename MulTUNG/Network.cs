using MulTUNG.Packeting.Packets;
using MulTUNG.Packeting.Packets.Utils;
using MulTUNG.Utils;
using PiTung;
using PiTung.Console;
using Server;
using System.Threading;
using UnityEngine;

namespace MulTUNG
{
    public static class Network
    {
        public static bool Connected => IsClient || IsServer;
        public static bool IsClient => NetworkClient.Instance?.Connected ?? false;
        public static bool IsServer => NetworkServer.Instance?.Running ?? false;

        public static bool IsPaused { get; private set; }

        public static int PlayerID => IsClient ? NetworkClient.Instance.PlayerID : ServerPlayerID;
        public static string Username => IsClient ? NetworkClient.Instance.Username : ServerUsername;

        public static string ServerUsername { get; set; } = "Server";

        public const int ServerPlayerID = 0;
        
        public static void ProcessPacket(Packet packet, int playerId)
        {
            try
            {
                if (packet == null)
                    return;

                if (packet is PlayerWelcomePacket wel)
                {
                    ServerUsername = wel.ServerUsername;
                    NetworkClient.Instance.SetID(wel.YourID);

                    SendPacket(new PlayerDataPacket
                    {
                        Username = NetworkClient.Instance.Username
                    });

                    return;
                }

                if (packet.SenderID == playerId && !(packet is StateListPacket))
                    return;

                if (IsClient && !NetworkClient.Instance.IsInGameplay)
                {
                    IGConsole.Log("Hold on " + packet.GetType().Name);
                    NetworkClient.Instance.EnterEvent.WaitOne();
                    IGConsole.Log("There you go");
                }

                switch (packet)
                {
                    case PlayerStatePacket state:
                        if (IsServer)
                        {
                            NetworkServer.Instance.UpdatePlayerState(state);
                        }

                        break;
                    case StateListPacket states:
                        PlayerManager.UpdateStates(states);

                        break;
                    case SignalPacket signal:
                        HandleSignalPacket(signal);

                        break;
                    case PlayerDisconnectPacket disconnect:
                        PlayerManager.WaveGoodbye(disconnect.PlayerID);

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
                        NetUtilitiesComponent.Instance.Enqueue(new DeleteComponentJob(delComp));

                        break;
                    case PlaceWirePacket placeWire:
                        NetUtilitiesComponent.Instance.Enqueue(new PlaceWireJob(placeWire));

                        break;
                    case DeleteWirePacket deleteWire:
                        NetUtilitiesComponent.Instance.Enqueue(new DeleteWireJob(deleteWire));

                        break;
                    case RotateComponentPacket rotateComp:
                        NetUtilitiesComponent.Instance.Enqueue(new RotateComponentJob(rotateComp));

                        break;
                    case WorldDataPacket world:
                        try
                        {
                            MulTUNG.SynchronizationContext.Post(o => World.Deserialize(((WorldDataPacket)o).Data), world);
                        }
                        catch (System.Exception ex)
                        {
                            IGConsole.Log(ex);
                        }

                        break;
                    case UserInputPacket input:
                        MulTUNG.SynchronizationContext.Post(o => ComponentActions.DoAction((UserInputPacket)o), input);

                        break;
                    case ComponentDataPacket compdata:
                        MulTUNG.SynchronizationContext.Post(o => ComponentActions.DoData((ComponentDataPacket)o), compdata);
                        break;
                    case CircuitStatePacket circuit:
                        MulTUNG.SynchronizationContext.Post(o => ComponentActions.UpdateStates((CircuitStatePacket)o), circuit);

                        break;
                    case PlayerDataPacket player:
                        if (IsServer)
                            NetworkServer.Instance.ReceivedPlayerData(player);
                        else if (IsClient)
                            PlayerManager.NewPlayer(player.SenderID, player.Username);

                        break;
                    case ChatMessagePacket chat:
                        IGConsole.Log($"<b>{chat.Username}</b>: {chat.Text}");

                        break;
                }
            }
            catch (System.Exception ex)
            {
                IGConsole.Log("Process " + ex);
            }
        }

        public static void HandleSignalPacket(SignalPacket signal)
        {
            switch (signal.Data)
            {
                case SignalData.CircuitUpdate:
                    break;
                case SignalData.RequestWorld:
                    if (IsServer)
                        NetworkServer.Instance.SendWorld(signal.SenderID);

                    break;
                case SignalData.Pause:
                    IsPaused = true;
                    Time.timeScale = 0;

                    break;
                case SignalData.Resume:
                    IsPaused = false;
                    Time.timeScale = 1;

                    break;
            }
        }
        
        public static void StartPositionUpdateThread(int updateInterval)
        {
            //The timer will start after 500ms have elapsed
            Timer timer = null;
            timer = new Timer(_ =>
            {
                if (!Connected || ModUtilities.IsOnMainMenu)
                {
                    timer.Dispose();
                    return;
                }
                
                SendState();

                if (IsServer)
                {
                    NetworkServer.Instance.SendStatesToPlayers();
                }
            }, null, 500, updateInterval);
        }

        private static void SendState()
        {
            var packet = new PlayerStatePacket
            {
                PlayerID = PlayerID,
                Position = FirstPersonInteraction.FirstPersonCamera.transform.position,
                EulerAngles = FirstPersonInteraction.FirstPersonCamera.transform.eulerAngles
            };

            if (IsClient)
                Network.SendPacket(packet);
            else
                Network.ProcessPacket(packet, 0);
        }

        public static void PauseGame()
        {
            if (IsServer)
                Network.SendPacket(new SignalPacket(SignalData.Pause));
        }

        public static void ResumeGame()
        {
            if (IsServer)
                Network.SendPacket(new SignalPacket(SignalData.Resume));
        }

        public static void SendPacket(Packet packet)
        {
            packet.Time = Time.time;

            if (IsClient)
            {
                NetworkClient.Instance.Send(packet);
            }
            else if (IsServer)
            {
                NetworkServer.Instance.Send(packet);
            }
        }
    }
}
