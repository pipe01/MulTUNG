using MulTUNG.Packets;
using MulTUNG.Utils;
using PiTung;

namespace MulTUNG.Patches
{

    [Target(typeof(Switch))]
    internal static class SwitchPatch
    {
        [PatchMethod]
        public static void UpdateLever(Switch __instance)
        {
            var netObj = __instance.transform.parent.GetComponent<NetObject>();

            if (netObj == null || ComponentActions.CurrentlyActing.Contains(netObj.NetID))
                return;

            var packet = new UserInputPacket
            {
                NetID = netObj.NetID,
                Receiver = UserInputPacket.UserInputReceiver.Switch,
                State = __instance.On
            };

            Network.SendPacket(packet);
        }
    }
}
