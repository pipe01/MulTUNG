﻿using MulTUNG.Headless;
using MulTUNG.Packeting.Packets;
using MulTUNG.UI;
using MulTUNG.Utils;
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
        public override string Name => "MulTUNG";
        public override string PackageName => "me.pipe01.MulTUNG";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.1");
        public override string UpdateUrl => "http://pipe0481.heliohost.org/pitung/mods/manifest.ptm";

        public const string ForbiddenSaveName = "_multiplayer";

        public static SynchronizationContext SynchronizationContext;
        public static bool ShowStatusWindow;
        public static string Status = "";

        private IDialog ConnectDialog;

        public override void BeforePatch()
        {
            if (Headlesser.IsHeadless)
            {
                HeadlessServer.Instance.Start();
            }
            
            IGConsole.RegisterCommand<Command_connect>();
            IGConsole.RegisterCommand<Command_disconnect>();
            IGConsole.RegisterCommand<Command_host>();
            IGConsole.RegisterCommand<Command_netobjs>();
            IGConsole.RegisterCommand<Command_chat>();
            IGConsole.RegisterCommand<Command_players>();
            IGConsole.RegisterCommand<Command_stop>();

            World.DeleteSave();
            SynchronizationContext = SynchronizationContext.Current;
        }

        public override void AfterPatch()
        {
            ConnectDialog = new ConnectDialog { Visible = false };
        }

        public override void Update()
        {
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

            if (Network.IsPaused)
            {
                Rect size = new Rect(0, 0, 200, 70);
                size.x = Screen.width / 2 - size.width / 2;
                size.y = Screen.height / 2 - size.height / 2;

                GUI.Box(size, "A player is downloading the world...");
            }

            if (ShowStatusWindow)
            {
                var rect = new Rect(0, 0, 300, 200);
                rect.x = Screen.width / 2 - rect.width / 2;
                rect.y = Screen.height / 2 - rect.height / 2;

                GUI.Window(53451, rect, DrawWindow, "");
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

            SetMainMenuCanvases(false);
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

        public static void SetMainMenuCanvases(bool enabled)
        {
            RunMainMenu.Instance.AboutCanvas.enabled = enabled;
            RunMainMenu.Instance.DeleteGameCanvas.enabled = enabled;
            RunMainMenu.Instance.LoadGameCanvas.enabled = enabled;
            RunMainMenu.Instance.MainMenuCanvas.enabled = enabled;
            RunMainMenu.Instance.NewGameCanvas.enabled = enabled;
            RunMainMenu.Instance.OptionsCanvas.enabled = enabled;
            RunMainMenu.Instance.RenameGameCanvas.enabled = enabled;
        }
    }

    public class Command_disconnect : Command
    {
        public override string Name => "disconnect";
        public override string Usage => Name;

        public override bool Execute(IEnumerable<string> arguments)
        {
            NetworkClient.Instance.Disconnect();

            return true;
        }
    }

    public class Command_connect : Command
    {
        public override string Name => "connect";
        public override string Usage => Name;

        public override bool Execute(IEnumerable<string> arguments)
        {
            MulTUNG.Connect(new IPEndPoint(IPAddress.Parse(arguments.FirstOrDefault() ?? "127.0.0.1"), Constants.Port));

            return true;
        }
    }

    public class Command_host : Command
    {
        public override string Name => "host";
        public override string Usage => $"{Name} [username]";

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

        public override bool Execute(IEnumerable<string> arguments)
        {
            if (!arguments.Any())
                return false;

            string msg = string.Join(" ", arguments.ToArray());

            IGConsole.Log($"<b>{Network.Username}</b>: {msg}");

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
