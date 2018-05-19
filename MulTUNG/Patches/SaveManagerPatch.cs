using PiTung;

namespace MulTUNG.Patches
{

    [Target(typeof(SaveManager))]
    internal static class SaveManagerPatch
    {
        [PatchMethod]
        static bool SaveAllSynchronously() => !Network.IsClient;
    }
}
