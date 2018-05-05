using MulTUNG.Client;
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

        private static IDictionary<int, RemotePlayer> Players = new Dictionary<int, RemotePlayer>();
        
        public static void NewPlayer(int id)
        {
            if (!Players.ContainsKey(id))
            {
                var obj = MakePlayerModel();

                Players.Add(id, obj.AddComponent<RemotePlayer>());
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

            player.UpdateWithPacket(packet);
        }

        public static void WaveGoodbye(int playerId)
        {
            if (Players.TryGetValue(playerId, out var player))
            {
                GameObject.Destroy(player.gameObject);

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
    }
}
