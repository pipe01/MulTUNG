using PiTung;

namespace MulTUNG.Patches
{

    [Target(typeof(Label))]
    internal static class LabelPatch
    {
        [PatchMethod]
        public static void Interact(Label __instance)
        {
            PatchesCommon.LabelBeingEdited = __instance;
        }
    }
}
