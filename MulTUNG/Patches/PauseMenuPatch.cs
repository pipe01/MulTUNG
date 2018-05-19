using PiTung;
using UnityEngine;

namespace MulTUNG.Patches
{

    [Target(typeof(PauseMenu))]
    internal static class PauseMenuPatch
    {
        [PatchMethod]
        public static bool QuitToMainMenu()
        {
            if (Network.IsClient)
            {
                NetworkClient.Instance.Disconnect();
                Time.timeScale = 1;

                return false;
            }

            return true;
        }
    }
}
