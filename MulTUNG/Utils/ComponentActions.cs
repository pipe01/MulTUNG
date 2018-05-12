using MulTUNG.Packeting.Packets;
using PiTung;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MulTUNG.Utils
{
    public static class ComponentActions
    {
        public static List<int> CurrentlyActing { get; } = new List<int>();

        public static void DoAction(UserInputPacket packet)
        {
            var obj = NetObject.GetByNetId(packet.NetID);

            CurrentlyActing.Add(packet.NetID);

            switch (packet.Receiver)
            {
                case UserInputPacket.UserInputReceiver.Button:
                    DoButton(obj.GetComponent<Button>(), packet.State);
                    break;
                case UserInputPacket.UserInputReceiver.Switch:
                    DoSwitch(obj.GetComponent<Switch>(), packet.State);
                    break;
            }

            CurrentlyActing.Remove(packet.NetID);
        }

        private static void DoButton(Button button, bool state)
        {
            if (state)
                ModUtilities.ExecuteMethod(button, "ButtonDown");
            else
                ModUtilities.ExecuteMethod(button, "ButtonUp");
        }

        private static void DoSwitch(Switch @switch, bool state)
        {
            @switch.On = state;
            @switch.UpdateLever();
        }
    }
}
