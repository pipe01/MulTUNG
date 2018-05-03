﻿using PiTung;
using PiTung.Console;
using System;
using System.Collections.Generic;

namespace Client
{
    public class Client : Mod
    {
        public override string Name => "MulTUNG";
        public override string PackageName => "me.pipe01.MulTUNG";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");

        internal static NetworkClient NetClient = new NetworkClient();

        public override void BeforePatch()
        {
            IGConsole.RegisterCommand<Command_connect>(this);
            IGConsole.RegisterCommand<Command_disconnect>(this);
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
            Client.NetClient.Disconnect();

            return true;
        }
    }

    public class Command_connect : Command
    {
        public override string Name => "connect";
        public override string Usage => Name;

        public override bool Execute(IEnumerable<string> arguments)
        {
            //string host = arguments.First();

            Client.NetClient.Connect("localhost");

            return true;
        }
    }
}
