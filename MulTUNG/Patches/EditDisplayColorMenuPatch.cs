using MulTUNG.Packets;
using PiTung;
using System.Collections.Generic;

namespace MulTUNG.Patches
{

    [Target(typeof(EditDisplayColorMenu))]
    internal static class EditDisplayColorMenuPatch
    {
        [PatchMethod(PatchType.Postfix)]
        public static void DoneMenu(EditDisplayColorMenu __instance)
        {
            var display = __instance.DisplayBeingEdited;
            var netObj = display.transform.parent.GetComponent<NetObject>();

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
