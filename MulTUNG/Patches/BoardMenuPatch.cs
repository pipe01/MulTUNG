using PiTung;

namespace MulTUNG.Patches
{

    [Target(typeof(BoardMenu))]
    internal static class BoardMenuPatch
    {
        [PatchMethod]
        public static void ExecuteSelectedAction()
        {
            if (BoardMenu.Instance.SelectedThing == 2)
            {
                PatchesCommon.IsTryingToMoveBoard = true;
            }
        }

        [PatchMethod("ExecuteSelectedAction", PatchType.Postfix)]
        public static void ExecuteSelectedActionPostfix()
        {
            PatchesCommon.IsTryingToMoveBoard = false;
        }
    }
}
