﻿using PiTung;
using SavedObjects;
using System.Linq;
using UnityEngine;

namespace MulTUNG.Patches
{

    [Target(typeof(SavedObjectUtilities))]
    internal static class SavedObjectUtilitiesPatch
    {
        [PatchMethod]
        public static bool LoadSavedObject(SavedObjectV2 save)
        {
            return !(save is SavedNetObject);
        }

        [PatchMethod("LoadSavedObject", PatchType.Postfix)]
        public static void LoadSavedObject(SavedObjectV2 save, ref GameObject __result)
        {
            var net = save.Children?.SingleOrDefault(o => o is SavedNetObject) as SavedNetObject;

            if (net != null)
            {
                var netObj = __result.GetComponent<NetObject>() ?? __result.AddComponent<NetObject>();
                netObj.NetID = net.NetID;
            }
        }
    }
}
