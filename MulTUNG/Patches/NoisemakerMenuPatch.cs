using MulTUNG.Packets;
using PiTung;
using System.Collections.Generic;

namespace MulTUNG.Patches
{

    [Target(typeof(NoisemakerMenu))]
    internal static class NoisemakerMenuPatch
    {
        [PatchMethod(PatchType.Postfix)]
        public static void Done()
        {
            var noisemaker = NoisemakerMenu.NoisemakerBeingEdited;
            var netObj = noisemaker.transform.parent.GetComponent<NetObject>();
            
            if (netObj == null)
                return;

            Network.SendPacket(new ComponentDataPacket
            {
                NetID = netObj.NetID,
                ComponentType = ComponentType.Noisemaker,
                Data = new List<object>
                {
                    noisemaker.ToneFrequency
                }
            });
        }
    }
}
