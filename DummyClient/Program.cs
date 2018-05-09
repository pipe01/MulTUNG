#define NET_462

using Harmony;
using MulTUNG;
using MulTUNG.Packeting.Packets;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args) => new Program().Run();

        public static Stopwatch TimeStopwatch = Stopwatch.StartNew();

        HarmonyInstance Harmony = HarmonyInstance.Create("dummyclient");

        public static void TestMethod()
        {
            Console.WriteLine("Not patched!");
        }

        public void Run()
        {
            HarmonyInstance.DEBUG = true;
            
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            Console.Write("Host (127.0.0.1): ");
            string host = Console.ReadLine();

            if (string.IsNullOrEmpty(host?.Trim()))
                host = "127.0.0.1";

            Console.WriteLine("Connecting...");

            new NetUtilitiesComponent();

            NetworkClient.Instance.Connect(host);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(1000);
                NetworkClient.Instance.Send(new PlayerStatePacket
                {
                    PlayerID = NetworkClient.Instance.PlayerID,
                    Position = Vector3.zero,
                    EulerAngles = Vector3.zero
                });
            });

            Thread.Sleep(1000);

            while (NetworkClient.Instance.Connected)
            {
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.Write("> ");

                    Log.PrintLines = false;

                    string line = Console.ReadLine();
                    Log.PrintLines = true;

                    ParseCommand(line);

                    Log.FlushQueue();
                }
            }
        }

        private void ParseCommand(string cmd)
        {
            switch (cmd.Split(' ')[0])
            {
                case "disconnect":
                    NetworkClient.Instance.Disconnect();
                    break;
                case "move":
                    string[] split = cmd.Split(' ');
                    float x = float.Parse(split[1]);
                    float y = float.Parse(split[2]);
                    float z = float.Parse(split[3]);

                    NetworkClient.Instance.Send(new PlayerStatePacket
                    {
                        PlayerID = NetworkClient.Instance.PlayerID,
                        Position = new Vector3(x, y, z),
                        EulerAngles = Vector3.zero
                    });

                    break;
            }
        }
    }
}
