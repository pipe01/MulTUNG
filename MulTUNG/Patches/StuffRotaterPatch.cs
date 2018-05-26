using MulTUNG.Packets;
using PiTung;
using UnityEngine;

namespace MulTUNG.Patches
{

    [Target(typeof(StuffRotater))]
    internal static class StuffRotaterPatch
    {
        [PatchMethod]
        public static void RotateThing(GameObject RotateThis, ref Vector3 __state)
        {
            __state = RotateThis.transform.localEulerAngles;
        }

        [PatchMethod("RotateThing", PatchType.Postfix)]
        public static void RotateThingPostfix(GameObject RotateThis, ref Vector3 __state)
        {
            if (RotateThis.transform.localEulerAngles != __state && RotateThis.tag != "Wire")
            {
                var netObj = RotateThis.GetComponent<NetObject>();

                if (netObj != null)
                {
                    Network.SendPacket(new RotateComponentPacket
                    {
                        ComponentID = netObj.NetID,
                        EulerAngles = RotateThis.transform.localEulerAngles
                    });
                }
            }
        }
    }
}
