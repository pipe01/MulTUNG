using MulTUNG.Packets;
using PiTung;
using System;
using System.Collections.Generic;

namespace MulTUNG.Patches
{

    [Target(typeof(EditDisplayColorMenu))]
    internal static class EditDisplayColorMenuPatch
    {
        [PatchMethod]
        public static bool RunDisplayColorMenu(EditDisplayColorMenu __instance)
        {
            try
            {
                var b = __instance.transform.parent;
            }
            catch (NullReferenceException)
            {
                __instance.DoneMenu();
                return false;
            }

            return true;
        }

        [PatchMethod(PatchType.Postfix)]
        public static void DoneMenu(EditDisplayColorMenu __instance)
        {
            var display = __instance.DisplayBeingEdited;
            NetObject netObj = null;

            try
            {
                netObj = display.transform.parent.GetComponent<NetObject>();
            }
            catch (System.NullReferenceException) { } //No, I can't null check

            if (netObj == null)
                return;

            Network.SendPacket(new ComponentDataPacket
            {
                NetID = netObj.NetID,
                ComponentType = ComponentType.Display,
                Data = new List<object>
                {
                    display.DisplayColor
                }
            });
        }
    }
}
