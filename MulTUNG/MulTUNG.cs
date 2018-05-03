using PiTung;
using PiTung.Console;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
            
            return true;
        }
    }
}
