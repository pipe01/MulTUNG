using MulTUNG.Packeting.Packets;
using MulTUNG.Utils;
using PiTung;

namespace MulTUNG.Patches
{

    [Target(typeof(BehaviorManager))]
    internal static class BehaviorManagerPatch
    {
        [PatchMethod]
        public static bool OnCircuitLogicUpdate()
        {
            if (ComponentActions.HasCalledCircuitUpdate)
            {
                ComponentActions.HasCalledCircuitUpdate = false;
                return true;
            }

            return !Network.IsClient;
        }

        [PatchMethod("OnCircuitLogicUpdate", PatchType.Postfix)]
        public static void OnCircuitLogicUpdatePostfix()
        {
            if (Network.IsServer)
            {
                Network.SendPacket(CircuitStatePacket.Build());
            }
        }
    }
}
