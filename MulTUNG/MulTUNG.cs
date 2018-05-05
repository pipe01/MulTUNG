using MulTUNG.Packeting.Packets;
using PiTung;
using PiTung.Console;
using Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public const string ForbiddenSaveName = "you shouldn't be seeing this";

        internal static NetworkClient NetClient = new NetworkClient();

        public override void BeforePatch()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            IGConsole.RegisterCommand<Command_connect>(this);
            IGConsole.RegisterCommand<Command_disconnect>(this);
            IGConsole.RegisterCommand<Command_host>(this);
#pragma warning restore CS0618 // Type or member is obsolete

            string path = Application.persistentDataPath + "/saves/" + ForbiddenSaveName;
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public override void Update()
        {
            if (ModUtilities.DummyComponent.gameObject.GetComponent<NetUtilitiesComponent>() == null)
                ModUtilities.DummyComponent.gameObject.AddComponent<NetUtilitiesComponent>();

            if (Input.GetKeyDown(KeyCode.U))
                IGConsole.Log(FirstPersonInteraction.FirstPersonCamera.transform.position);
        }

        public override void OnGUI()
        {
            if (!ModUtilities.IsOnMainMenu)
                ModUtilities.Graphics.DrawText(Time.time.ToString("0.00"), new Vector2(3, 3), Color.white, true);
        }

        public override void OnApplicationQuit()
        {
            NetClient.Disconnect();
        }
    }

    public class Command_disconnect : Command
    {
        public override string Name => "disconnect";
        public override string Usage => Name;

        public override bool Execute(IEnumerable<string> arguments)
        {
            MulTUNG.NetClient.Disconnect();

            return true;
        }
    }

    public class Command_connect : Command
    {
        public override string Name => "connect";
        public override string Usage => Name;

        public override bool Execute(IEnumerable<string> arguments)
        {
            if (!ModUtilities.IsOnMainMenu)
            {
                return true;
            }

            SaveManager.SaveName = MulTUNG.ForbiddenSaveName;
            SceneManager.LoadScene("gameplay");
            EverythingHider.HideEverything();

            SceneManager.sceneLoaded += (a, b) =>
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Thread.Sleep(3000);
                    MulTUNG.NetClient.Connect(arguments.FirstOrDefault() ?? "127.0.0.1");
                });
            };

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
            
            //var obj = GameObject.Instantiate(PlayerManager.PlayerModelPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            //obj.SetActive(true);

            //var collider = obj.AddComponent<MeshCollider>();
            //collider.sharedMesh = new ObjImporter().ImportFile(Properties.Resources.patrick);
            //collider.tag = "World";
            //obj.layer = LayerMask.NameToLayer("World");

            return true;
        }
    }
}
