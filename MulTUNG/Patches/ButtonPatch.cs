﻿using MulTUNG.Packeting.Packets;
using MulTUNG.Utils;
using PiTung;

namespace MulTUNG.Patches
{

    [Target(typeof(Button))]
    internal static class ButtonPatch
    {
        [PatchMethod]
        public static bool Update(Button __instance)
        {
            if (ComponentActions.PushedDownButtons.Contains(__instance))
                return false;

            return true;
        }

        [PatchMethod(PatchType.Postfix)]
        public static void ButtonDown(Button __instance) => DoButton(__instance);

        [PatchMethod(PatchType.Postfix)]
        public static void ButtonUp(Button __instance) => DoButton(__instance);

        private static void DoButton(Button __instance)
        {
            var netObj = __instance.transform.parent.GetComponent<NetObject>();

            if (netObj == null || ComponentActions.CurrentlyActing.Contains(netObj.NetID))
                return;

            var packet = new UserInputPacket
            {
                NetID = netObj.NetID,
                Receiver = UserInputPacket.UserInputReceiver.Button,
                State = __instance.output.On
            };

            Network.SendPacket(packet);
        }
    }
}