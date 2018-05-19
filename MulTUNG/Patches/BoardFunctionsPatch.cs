using PiTung;

namespace MulTUNG.Patches
{

    [Target(typeof(BoardFunctions))]
    internal static class BoardFunctionsPatch
    {
        [PatchMethod]
        public static void CloneBoard()
        {
            PatchesCommon.IsCloning = true;
        }

        [PatchMethod("CloneBoard", PatchType.Postfix)]
        public static void CloneBoardPostfix()
        {
            PatchesCommon.IsCloning = false;
        }
    }
}
