using MulTUNG.Client;
using MulTUNG.Packeting.Packets;
using PiTung.Console;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MulTUNG
{
    public static class PlayerManager
    {
        private class PlayerStruct
        {
            public GameObject Object { get; set; }
            public float LastUpdateTime { get; set; }

            public PlayerStruct(GameObject obj)
            {
                this.Object = obj;
            }
        }

        public static GameObject PlayerModelPrefab { get; private set; }

        private static IDictionary<int, RemotePlayer> PlayersInner = new Dictionary<int, RemotePlayer>();

        public static RemotePlayer[] Players => PlayersInner.Values.ToArray();

        public static void NewPlayer(int id, string username)
        {
            if (PlayersInner.TryGetValue(id, out var player))
            {
                player.Username = username;
            }
            else
            {
                IGConsole.Log($"<color=orange>Player {username} connected</color>");

                //Instantiate a new player from the prefab
                var obj = MakePlayerModel();

                //Add a RemotePlayer component
                var remotePlayer = obj.AddComponent<RemotePlayer>();
                remotePlayer.Username = username;
                
                //Add it to the players registry
                PlayersInner.Add(id, remotePlayer);
            }
        }

        public static void UpdateStates(StateListPacket states)
        {
            foreach (var item in PlayersInner.Where(o => !states.States.ContainsKey(o.Key)))
            {
                PlayersInner.Remove(item.Key);
            }

            foreach (var item in states.States)
            {
                if (item.Key == Network.PlayerID)
                    continue;

                UpdatePlayer(item.Value, false);
            }
        }

        public static void UpdatePlayer(PlayerState state, bool create = false)
        {
            //If the player doesn't exist
            if (!PlayersInner.ContainsKey(state.PlayerID))
            {
                if (create)
                {
                    //Create it
                    NewPlayer(state.PlayerID, state.Username);
                }
                else
                {
                    return;
                }
            }

            var player = PlayersInner[state.PlayerID];

            //If this packet somehow has an earlier time than the last received packet, skip it
            if (state.Time < player.LastUpdateTime)
                return;

            //Update the RemotePlayer
            player.UpdateState(state);
        }

        public static void WaveGoodbye(int playerId)
        {
            if (PlayersInner.TryGetValue(playerId, out var player))
            {
                IGConsole.Log($"<color=orange>Player {player.Username} disconnected</color>");

                GameObject.Destroy(player.gameObject);

                PlayersInner.Remove(playerId);
            }
        }

        private static GameObject MakePlayerModel()
        {
            if (PlayerModelPrefab == null)
            {
                //Load model on Unity's main thread
                MulTUNG.SynchronizationContext.Send(_ => BuildPlayerPrefab(), null);

                //Check if the prefab is still null
                if (PlayerModelPrefab == null)
                {
                    IGConsole.Error("Couldn't load player model!");
                    return null;
                }
            }

            //Create a new parent object that will contain the model
            GameObject player = new GameObject("Remote Player");
            GameObject newModel = GameObject.Instantiate(PlayerModelPrefab, player.transform);
            newModel.SetActive(true);

            //Offset its local position by half of the model height
            newModel.transform.localPosition = new Vector3(0, -1.65f / 2, 0);
            
            return player;
        }

        public static void BuildPlayerPrefab()
        {
            OBJLoader.OBJFile file = new OBJLoader.OBJFile
            {
                ObjData = Properties.Resources.patrick,
                MtlData = Properties.Resources.patrick_mtl,
                MtlImages = new Dictionary<string, byte[]>()
                {
                    ["Image_2D_0001_0008.png"] = Properties.Resources.Image_2D_0001_0008,
                    ["Image_2D_0002_0009.png"] = Properties.Resources.Image_2D_0002_0009,
                    ["Image_2D_0003_0010.png"] = Properties.Resources.Image_2D_0003_0010,
                }
            };

            PlayerModelPrefab = OBJLoader.LoadOBJFile("Patrick", file);
            PlayerModelPrefab.transform.position = new Vector3(-1000, -1000, -1000);
            PlayerModelPrefab.transform.localScale = new Vector3(.025f, .025f, .025f);
        }

        public static void Reset()
        {
            foreach (var item in PlayersInner)
            {
                GameObject.Destroy(item.Value.gameObject);
            }

            PlayersInner.Clear();
        }
    }
}
