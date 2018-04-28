using Common.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Client
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

        private static GameObject PlayerModelPrefab;

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

            if (!packet.Connected)
            {
                GameObject.Destroy(player.Object);

                Players.Remove(packet.PlayerID);

                return;
            }
            
            if (packet.Time < player.LastUpdateTime)
                return;

            player.Object.transform.position = packet.Position;
            player.Object.transform.eulerAngles = packet.EulerAngles;
        }

        private static GameObject MakePlayerModel()
        {
            if (PlayerModelPrefab == null)
            {
                PlayerModelPrefab = new GameObject("Remote Player");

                MeshFilter filter = PlayerModelPrefab.AddComponent<MeshFilter>();
                filter.mesh = MakePlayerMesh();

                MeshRenderer renderer = PlayerModelPrefab.AddComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Diffuse"));
                renderer.material.color = Color.white;

                PlayerModelPrefab.SetActive(false);
            }

            GameObject newModel = GameObject.Instantiate(PlayerModelPrefab);
            newModel.SetActive(true);

            newModel.GetComponent<MeshRenderer>().material.color = UnityEngine.Random.ColorHSV();

            return newModel;
        }

        private static Mesh MakePlayerMesh()
        {
            //Blatantly stolen from @Stenodyon with love ;-)

            Vector3[] vertices = {
                new Vector3(-0.5f, -0.5f, -0.5f), // 0
                new Vector3( 0.5f, -0.5f, -0.5f), // 1
                new Vector3( 0.5f, -0.5f,  0.5f), // 2
                new Vector3(-0.5f, -0.5f,  0.5f), // 3
                new Vector3(-0.5f,  0.5f, -0.5f), // 4
                new Vector3( 0.5f,  0.5f, -0.5f), // 5
                new Vector3( 0.5f,  0.5f,  0.5f), // 6
                new Vector3(-0.5f,  0.5f,  0.5f), // 7
            };

            int[] triangles = {
                // Top (4, 5, 6, 7)
                4, 6, 5,
                4, 7, 6,
                // Front (0, 1, 4, 5)
                0, 5, 1,
                0, 4, 5,
                // Back (2, 3, 6, 7)
                2, 7, 3,
                2, 6, 7,
                // Left (0, 3, 4, 7)
                3, 4, 0,
                3, 7, 4,
                // Right (1, 2, 5, 6)
                1, 6, 2,
                1, 5, 6,
                // Bottom (0, 1, 2, 3)
                0, 1, 2,
                0, 2, 3,
            };

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
