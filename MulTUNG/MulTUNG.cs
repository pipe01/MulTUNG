using MulTUNG.Packeting.Packets;
using PiTung;
using PiTung.Console;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace MulTUNG
{
    public class MulTUNG : Mod
    {
        public override string Name => "MulTUNG";
        public override string PackageName => "me.pipe01.MulTUNG";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");

        internal static NetworkClient NetClient = new NetworkClient();

        public override void BeforePatch()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            IGConsole.RegisterCommand<Command_connect>(this);
            IGConsole.RegisterCommand<Command_disconnect>(this);
            IGConsole.RegisterCommand<Command_host>(this);
#pragma warning restore CS0618 // Type or member is obsolete
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
            MulTUNG.NetClient.Connect(arguments.FirstOrDefault() ?? "127.0.0.1");

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

            ModUtilities.Graphics.CreateSphere(new Vector3(0, 0, 0), 2);

            var obj = GameObject.Instantiate(PlayerManager.PlayerModelPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            obj.SetActive(true);

            var collider = obj.AddComponent<MeshCollider>();
            collider.sharedMesh = new ObjImporter().ImportFile(Properties.Resources.patrick);
            collider.tag = "World";
            obj.layer = LayerMask.NameToLayer("World");

            return true;
        }
    }
}
