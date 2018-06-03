using MulTUNG.Headless;
using MulTUNG.Packets;
using MulTUNG.UI;
using MulTUNG.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using PiTung;
using PiTung.Console;
using PiTung.Mod_utilities;
using Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MulTUNG
{
    public class MulTUNG : Mod
    {
        public static readonly Version Version = new Version("1.0.3");

        public override string Name => "MulTUNG";
        public override string PackageName => "me.pipe01.MulTUNG";
        public override string Author => "pipe01";
        public override Version ModVersion => Version;
        public override string UpdateUrl => "http://pipe0481.heliohost.org/pitung/mods/manifest.ptm";

        public const string ForbiddenSaveName = "_multiplayer";

        public static SynchronizationContext SynchronizationContext;
        public static bool ShowStatusWindow;
        public static string Status = "";
        public static AudioClip PopAudio { get; private set; }

        private IDialog ConnectDialog;

        private static bool[] CanvasStatuses = new bool[7];

        public override void BeforePatch()
        {
            if (!Headlesser.IsHeadless)
            {
                LoadChatAudio();
            }
            
            Shell.RegisterCommand<Command_connect>();
            Shell.RegisterCommand<Command_disconnect>();
            Shell.RegisterCommand<Command_host>();
            Shell.RegisterCommand<Command_netobjs>();
            Shell.RegisterCommand<Command_chat>();
            Shell.RegisterCommand<Command_players>();
            Shell.RegisterCommand<Command_stop>();

            World.DeleteSave();
            SynchronizationContext = SynchronizationContext.Current;
        }

        public override void AfterPatch()
        {
            ConnectDialog = new ConnectDialog { Visible = false };
        }

        public override void Update()
        {
            //IGConsole.Log($"Client: {Network.IsClient}; Server: {Network.IsServer}");

            if (ModUtilities.DummyComponent.gameObject.GetComponent<NetUtilitiesComponent>() == null)
                ModUtilities.DummyComponent.gameObject.AddComponent<NetUtilitiesComponent>();
        }

        public override void OnGUI()
        {
            if (ModUtilities.IsOnMainMenu && (RunMainMenu.Instance?.MainMenuCanvas?.enabled ?? false))
            {
                if (GUI.Button(new Rect(Screen.width - 3 - 80, 3, 80, 35), "Connect"))
                {
                    ConnectDialog.Visible = !ConnectDialog.Visible;
                }

                ConnectDialog.Draw();
            }

            if (ShowStatusWindow)
            {
                var rect = new Rect(0, 0, 300, 200);
                rect.x = Screen.width / 2 - rect.width / 2;
                rect.y = Screen.height / 2 - rect.height / 2;
                
                GUI.Window(53451, rect, DrawWindow, "");
            }

            if (!ModUtilities.IsOnMainMenu && PauseMenu.Instance.PauseCanvas.enabled && Network.Running)
            {
                PlayersList.Instance.Draw();
            }
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    GUILayout.Label(Status);

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
        }

        private void LoadChatAudio()
        {
            using (var fileStream = new MemoryStream(Properties.Resources.chatmessage))
            {
                WaveStream readerStream = new Mp3FileReader(fileStream);
                SampleChannel sampleChannel = new SampleChannel(readerStream);

                int destBytesPerSample = 4 * sampleChannel.WaveFormat.Channels;
                int sourceBytesPerSample = (readerStream.WaveFormat.BitsPerSample / 8) * readerStream.WaveFormat.Channels;
                int byteLength = (int)(destBytesPerSample * (readerStream.Length / sourceBytesPerSample));

                float[] audioFile = new float[byteLength / sizeof(float)];
                sampleChannel.Read(audioFile, 0, audioFile.Length);

                PopAudio = AudioClip.Create("test.mp3", byteLength, sampleChannel.WaveFormat.Channels, sampleChannel.WaveFormat.SampleRate, false);
                PopAudio.SetData(audioFile, 0);
            }
        }

        public static void PlayChatPop()
        {
            SoundPlayer.PlaySoundGlobal(PopAudio);
        }

        public override void OnApplicationQuit()
        {
            NetworkClient.Instance?.Disconnect();
            NetworkServer.Instance?.Stop();
        }

        public static void Connect(IPEndPoint endpoint)
        {
            if (!ModUtilities.IsOnMainMenu)
            {
                return;
            }

            HideMainMenuCanvases();
            ShowStatusWindow = true;
            Status = "Connecting to server...";

            NetworkClient.Instance.Connect(endpoint);
        }

        public static void DumpNetobjs()
        {
            var scene = SceneManager.GetActiveScene();

            Console.WriteLine($"-----BEGIN NET OBJECT DUMP----- [{(Network.IsClient ? "Client" : "Server")}]");
            Console.WriteLine("Alive NetObjects: " + NetObject.Alive.Count);

            foreach (var obj in scene.GetRootGameObjects())
            {
                Recurse(obj);
            }

            Console.WriteLine("------END NET OBJECT DUMP------");
            
            void Recurse(GameObject parent, int level = 0)
            {
                string indentation = new string(' ', level * 4);

                var objInfo = parent.GetComponent<ObjectInfo>();

                if (objInfo != null)
                {
                    var netObj = parent.GetComponent<NetObject>();

                    if (netObj != null)
                    {
                        Console.WriteLine($"{indentation}{objInfo.ComponentType} ID: {netObj.NetID}");
                    }
                }

                foreach (Transform item in parent.transform)
                {
                    Recurse(item.gameObject, level + 1);
                }
            }
        }

        public static void HideMainMenuCanvases()
        {
            CanvasStatuses[0] = RunMainMenu.Instance.AboutCanvas.enabled;
            CanvasStatuses[1] = RunMainMenu.Instance.DeleteGameCanvas.enabled;
            CanvasStatuses[2] = RunMainMenu.Instance.LoadGameCanvas.enabled;
            CanvasStatuses[3] = RunMainMenu.Instance.MainMenuCanvas.enabled;
            CanvasStatuses[4] = RunMainMenu.Instance.NewGameCanvas.enabled;
            CanvasStatuses[5] = RunMainMenu.Instance.OptionsCanvas.enabled;
            CanvasStatuses[6] = RunMainMenu.Instance.RenameGameCanvas.enabled;

            RunMainMenu.Instance.AboutCanvas.enabled = false;
            RunMainMenu.Instance.DeleteGameCanvas.enabled = false;
            RunMainMenu.Instance.LoadGameCanvas.enabled = false;
            RunMainMenu.Instance.MainMenuCanvas.enabled = false;
            RunMainMenu.Instance.NewGameCanvas.enabled = false;
            RunMainMenu.Instance.OptionsCanvas.enabled = false;
            RunMainMenu.Instance.RenameGameCanvas.enabled = false;
        }

        public static void ShowMainMenuCanvases()
        {
            RunMainMenu.Instance.AboutCanvas.enabled = CanvasStatuses[0];
            RunMainMenu.Instance.DeleteGameCanvas.enabled = CanvasStatuses[1];
            RunMainMenu.Instance.LoadGameCanvas.enabled = CanvasStatuses[2];
            RunMainMenu.Instance.MainMenuCanvas.enabled = CanvasStatuses[3];
            RunMainMenu.Instance.NewGameCanvas.enabled = CanvasStatuses[4];
            RunMainMenu.Instance.OptionsCanvas.enabled = CanvasStatuses[5];
            RunMainMenu.Instance.RenameGameCanvas.enabled = CanvasStatuses[6];
        }
    }

    public class Command_disconnect : Command
    {
        public override string Name => "disconnect";
        public override string Usage => Name;
        public override string Description => "Disconnects from the current server.";

        public override bool Execute(IEnumerable<string> arguments)
        {
            NetworkClient.Instance.Disconnect();

            return true;
        }
    }

    public class Command_connect : Command
    {
        public override string Name => "connect";
        public override string Usage => $"{Name} <ip>";
        public override string Description => "Connects to a host.";

        public override bool Execute(IEnumerable<string> arguments)
        {
            if (arguments.Count() != 1)
                return false;

            MulTUNG.Connect(new IPEndPoint(IPAddress.Parse(arguments.FirstOrDefault() ?? "127.0.0.1"), Constants.Port));

            return true;
        }
    }

    public class Command_host : Command
    {
        public override string Name => "host";
        public override string Usage => $"{Name} [username]";
        public override string Description => "Hosts a server on the current world with an optional username.";

        public override bool Execute(IEnumerable<string> arguments)
        {
            new NetworkServer().Start();
            
            if (arguments.Any())
            {
                Network.ServerUsername = arguments.First();
            }

            if (Configuration.Get("GetPublicAddress", true))
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    string ipStr = null;

                    try
                    {
                        ipStr = new WebClient().DownloadString("http://icanhazip.com/");
                    }
                    catch { }

                    if (ipStr != null && IPAddress.TryParse(ipStr.Trim(), out var ip))
                    {
                        Log.WriteLine("Your public IP address is " + ip);
                    }
                });
            }
            
            return true;
        }
    }

    public class Command_netobjs : Command
    {
        public override string Name => "netobjs";
        public override string Usage => Name;
        public override string Description => "Outputs a tree of net objects to the log.";

        public override bool Execute(IEnumerable<string> arguments)
        {
            MulTUNG.DumpNetobjs();

            return true;
        }
    }

    public class Command_chat : Command
    {
        public override string Name => "chat";
        public override string Usage => $"{Name} <message>";
        public override string[] Aliases => new[] { "c" };
        public override string Description => "Sends a chat message to everyone on the server.";

        public override bool Execute(IEnumerable<string> arguments)
        {
            if (!arguments.Any())
                return false;

            string msg = string.Join(" ", arguments.ToArray());
            
            Network.SendPacket(new ChatMessagePacket
            {
                Username = Network.Username,
                Text = msg
            });

            return true;
        }
    }

    public class Command_players : Command
    {
        public override string Name => "players";
        public override string Usage => Name;
        public override string Description => "Prints a list of connected players";

        public override bool Execute(IEnumerable<string> arguments)
        {
            IList<string> players = PlayerManager.Players.Select(o => o.Username).ToList();
            players.Insert(0, Network.Username);

            IGConsole.Log($"<color=lime>Online players</color> (<b>{players.Count}</b>): {string.Join(", ", players.ToArray())}");
            
            return true;
        }
    }

    public class Command_stop : Command
    {
        public override string Name => "stop";
        public override string Usage => Name;
        public override string Description => "Stops hosting a server.";

        public override bool Execute(IEnumerable<string> arguments)
        {
            if (Network.IsServer)
            {
                NetworkServer.Instance.Stop();

                EverythingHider.HideEverything();
                SceneManager.LoadScene("main menu");
            }

            return true;
        }
    }
}
