using MulTUNG.Packeting.Packets;
using PiTung;
using PiTung.Console;
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
        public static List<Button> PushedDownButtons { get; } = new List<Button>();

        public static void DoAction(UserInputPacket packet)
        {
            var obj = NetObject.GetByNetId(packet.NetID);

            CurrentlyActing.Add(packet.NetID);

            switch (packet.Receiver)
            {
                case UserInputPacket.UserInputReceiver.Button:
                    DoButton(obj.GetComponentInChildren<Button>(), packet.State);
                    break;
                case UserInputPacket.UserInputReceiver.Switch:
                    DoSwitch(obj.GetComponentInChildren<Switch>(), packet.State);
                    break;
            }

            CurrentlyActing.Remove(packet.NetID);
        }

        private static void DoButton(Button button, bool state)
        {
            if (state)
            {
                ModUtilities.ExecuteMethod(button, "ButtonDown");
                PushedDownButtons.Add(button);
            }
            else
            {
                ModUtilities.ExecuteMethod(button, "ButtonUp");
                PushedDownButtons.Remove(button);
            }
        }

        private static void DoSwitch(Switch @switch, bool state)
        {
            @switch.On = state;
            @switch.UpdateLever();
        }

        public static void DoData(ComponentDataPacket packet)
        {
            var netObj = NetObject.GetByNetId(packet.NetID);

            if (netObj == null)
                return;

            switch (packet.ComponentType)
            {
                case ComponentType.Noisemaker:
                    var noisemaker = netObj.GetComponentInChildren<Noisemaker>();
                    noisemaker.ToneFrequency = (float)packet.Data[0];

                    break;
                case ComponentType.Display:
                    var display = netObj.GetComponentInChildren<Display>();
                    display.DisplayColor = (DisplayColor)packet.Data[0];
                    display.ForceVisualRefresh();

                    break;
                case ComponentType.Label:
                    var label = netObj.GetComponent<Label>();
                    label.text.text = (string)packet.Data[0];
                    label.text.fontSize = (float)packet.Data[1];

                    break;
            }
        }
    }
}
