using MulTUNG.Packets;
using PiTung;
using System;
using System.Collections.Generic;

namespace MulTUNG.Patches
{

    [Target(typeof(TextEditMenu))]
    internal static class TextEditMenuPatch
    {
        [PatchMethod]
        public static void Done()
        {
            NetObject netObj;

            try
            {
                netObj = PatchesCommon.LabelBeingEdited.GetComponent<NetObject>();
            }
            catch (NullReferenceException)
            {
                return;
            }

            if (netObj == null)
                return;
            
            Network.SendPacket(new ComponentDataPacket
            {
                NetID = netObj.NetID,
                ComponentType = ComponentType.Label,
                Data = new List<object>
                {
                    PatchesCommon.LabelBeingEdited.text.text,
                    PatchesCommon.LabelBeingEdited.text.fontSize
                }
            });
        }
    }
}
