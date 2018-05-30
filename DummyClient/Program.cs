using Harmony;
using MulTUNG;
using MulTUNG.Packets;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace DummyClient
{
    class MySyncContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object state)
        {
        }

        public override void Send(SendOrPostCallback d, object state)
        {
        }
    }

    class Program
    {
        static void Main(string[] args) => new Program().Run();

        public static Stopwatch TimeStopwatch = Stopwatch.StartNew();

        private Vector3 Position, Rotation;
        private HarmonyInstance Harmony = HarmonyInstance.Create("dummyclient");
        
        public void Run()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            MulTUNG.MulTUNG.SynchronizationContext = new MySyncContext();
            
            AppDomain.CurrentDomain.UnhandledException += MyExceptionHandler;

            Console.Write("Host (127.0.0.1): ");
            string host = Console.ReadLine();

            if (string.IsNullOrEmpty(host?.Trim()))
                host = "127.0.0.1";

            Console.WriteLine("Connecting...");

            new NetUtilitiesComponent();

            NetworkClient.Instance.SetUsername("Dummy" + new System.Random().Next(0, 100));
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

            while (true)
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

        private void OpenSendForm()
        {
            ThreadPool.QueueUserWorkItem(_ => new frmSendPacket().ShowDialog());
        }

        private void MyExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine((e.ExceptionObject as Exception).Message);
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
                        Position = this.Position = new Vector3(x, y, z),
                        EulerAngles = this.Rotation
                    });

                    break;
                case "rotate":
                    string[] splita = cmd.Split(' ');
                    float xa = float.Parse(splita[1]);
                    float ya = float.Parse(splita[2]);
                    float za = float.Parse(splita[3]);

                    NetworkClient.Instance.Send(new PlayerStatePacket
                    {
                        PlayerID = NetworkClient.Instance.PlayerID,
                        Position = this.Position,
                        EulerAngles = this.Rotation = new Vector3(xa, ya, za)
                    });

                    break;
                case "send":
                    OpenSendForm();

                    break;
            }
        }
    }
}
