using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MulTUNG.Utils
{
    public static class Extensions
    {
        public static IList<T> PushToTop<T>(this IList<T> list, Func<T, bool> pushPredicate)
        {
            return (list.Where(pushPredicate).Concat(list.Where(o => !pushPredicate(o)))).ToList();
        }

        public static NetObject GetOrAddNetObject(this GameObject gameObject, bool setId = true)
        {
            var netObj = gameObject.GetComponent<NetObject>();

            if (netObj == null)
            {
                netObj = gameObject.AddComponent<NetObject>();

                if (setId)
                    netObj.NetID = NetObject.GetNewID();
            }

            return netObj;
        }
    }
}
