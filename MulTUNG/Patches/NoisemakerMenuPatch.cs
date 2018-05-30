using MulTUNG.Packets;
using PiTung;
using System;
using System.Collections.Generic;

namespace MulTUNG.Patches
{

    [Target(typeof(NoisemakerMenu))]
    internal static class NoisemakerMenuPatch
    {
        [PatchMethod]
        public static bool RunNoisemakerMenu()
        {
            try
            {
                var b = NoisemakerMenu.NoisemakerBeingEdited.Audio.isPlaying;
            }
            catch (NullReferenceException)
            {
                NoisemakerMenu.Instance.Done();
                return false;
            }

            return true;
        }

        [PatchMethod(PatchType.Postfix)]
        public static void Done()
        {
            var noisemaker = NoisemakerMenu.NoisemakerBeingEdited;
            NetObject netObj = null;

            try
            {
                netObj = noisemaker.transform.parent.GetComponent<NetObject>();
            }
            catch (System.NullReferenceException) { }

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
