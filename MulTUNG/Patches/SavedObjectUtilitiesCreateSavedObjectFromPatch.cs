using Harmony;
using SavedObjects;
using System;

namespace MulTUNG.Patches
{

    [HarmonyPatch(typeof(SavedObjectUtilities), "CreateSavedObjectFrom", new[] { typeof(ObjectInfo) })]
    internal static class SavedObjectUtilitiesCreateSavedObjectFromPatch
    {
        static void Postfix(ObjectInfo worldsave, ref SavedObjectV2 __result)
        {
            var netObj = worldsave.GetComponent<NetObject>();

            if (netObj != null)
            {
                if (__result.Children == null)
                    __result.Children = new SavedObjectV2[0];

                SavedObjectV2[] newChildren = new SavedObjectV2[__result.Children.Length + 1];
                Array.Copy(__result.Children, 0, newChildren, 1, __result.Children.Length);
                
                newChildren[0] = new SavedNetObject(netObj.NetID);

                __result.Children = newChildren;
            }
        }
    }
}
