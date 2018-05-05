using MulTUNG.Packeting.Packets;
using PiTung.Console;
using PiTung.Mod_utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MulTUNG
{
    public static class PlayerManager
    {
        private class RemotePlayer
        {
            public GameObject Object { get; set; }
            public float LastUpdateTime { get; set; }

            public RemotePlayer(GameObject obj)
            {
                this.Object = obj;
            }
        }

        public static GameObject PlayerModelPrefab { get; private set; }

        private static IDictionary<int, RemotePlayer> Players = new Dictionary<int, RemotePlayer>();
        
        public static void NewPlayer(int id)
        {
            if (!Players.ContainsKey(id))
            {
                Players.Add(id, new RemotePlayer(MakePlayerModel()));
            }
        }

        public static void UpdatePlayer(PlayerStatePacket packet)
        {
            if (!Players.ContainsKey(packet.PlayerID))
            {
                NewPlayer(packet.PlayerID);
            }

            var player = Players[packet.PlayerID];
            
            if (packet.Time < player.LastUpdateTime)
                return;

            player.Object.transform.position = packet.Position - new Vector3(0, 1.65f / 2, 0);
            player.Object.transform.eulerAngles = packet.EulerAngles;
        }

        public static void WaveGoodbye(int playerId)
        {
            if (Players.TryGetValue(playerId, out var player))
            {
                GameObject.Destroy(player.Object);

                Players.Remove(playerId);
            }
        }

        private static GameObject MakePlayerModel()
        {
            if (PlayerModelPrefab == null)
                return null;

            GameObject player = new GameObject("Remote Player");
            GameObject newModel = GameObject.Instantiate(PlayerModelPrefab, player.transform);
            newModel.SetActive(true);
            newModel.transform.localPosition = new Vector3(0, -1.65f / 2, 0);
            
            return player;
        }

        public static void BuildPlayerPrefab()
        {
            PlayerModelPrefab = OBJLoader.LoadOBJFile(@"C:\Users\Pipe\Downloads\Patrick\patrick.obj");
            PlayerModelPrefab.transform.position = new Vector3(-1000, -1000, -1000);
            PlayerModelPrefab.transform.localScale = new Vector3(.025f, .025f, .025f);
        }
    }
}
