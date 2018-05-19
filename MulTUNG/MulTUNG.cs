using MulTUNG.Headless;
using MulTUNG.UI;
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
        public override Version ModVersion => new Version("1.0.0");
        public override string UpdateUrl => "http://pipe0481.heliohost.org/pitung/mods/manifest.ptm";

        public const string ForbiddenSaveName = "you shouldn't be seeing this";

        public static SynchronizationContext SynchronizationContext;

        private IDialog ConnectDialog;

        //static MulTUNG()
        //{
        //    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        //}

        //private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    Console.WriteLine("::: " + args.Name);

        //    var dic = new Dictionary<string, byte[]>
        //    {
        //        ["Lidgren.Network"] = Properties.Resources.Lidgren_Network
        //    };

        //    var name = new AssemblyName(args.Name).Name;

        //    if (dic.TryGetValue(name, out var b))
        //    {
        //        return Assembly.Load(b);
        //    }

        //    return null;
        //}

        public override void BeforePatch()
        {
            if (Headlesser.IsHeadless)
            {
                HeadlessServer.Instance.Start();
            }

#pragma warning disable CS0618 // Type or member is obsolete
            IGConsole.RegisterCommand<Command_connect>(this);
            IGConsole.RegisterCommand<Command_disconnect>(this);
            IGConsole.RegisterCommand<Command_host>(this);
            IGConsole.RegisterCommand<Command_netobjs>(this);
#pragma warning restore CS0618 // Type or member is obsolete

            string path = Application.persistentDataPath + "/saves/" + ForbiddenSaveName;
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            
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
        }

        public override void OnApplicationQuit()
        {
            NetworkClient.Instance.Disconnect();
        }

        public static void Connect(IPEndPoint endpoint)
        {
            if (!ModUtilities.IsOnMainMenu)
            {
                return;
            }

            SaveManager.SaveName = ForbiddenSaveName;
            SceneManager.LoadScene("gameplay");
            EverythingHider.HideEverything();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                while (ModUtilities.IsOnMainMenu)
                    Thread.Sleep(500);

                Thread.Sleep(1000);

                NetworkClient.Instance.Connect(endpoint);
            });
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
        public override string Usage => Name;

        public override bool Execute(IEnumerable<string> arguments)
        {
            new NetworkServer().Start();
            
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
}
