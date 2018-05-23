using MulTUNG.Packeting.Packets;
using MulTUNG.Utils;
using PiTung;
using PiTung.Console;

namespace MulTUNG.Patches
{

    [Target(typeof(BehaviorManager))]
    internal static class BehaviorManagerPatch
    {
        public static int UpdateCounter = 0;

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
                bool full = UpdateCounter++ % Constants.SendFullCircuitEachXUpdates == 0;

                var packet = CircuitStatePacket.Build(full);

                if (packet.Count > 0)
                    Network.SendPacket(packet);
            }
        }
    }
}
