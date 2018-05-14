using System;
using System.Collections.Generic;
using System.Linq;

namespace MulTUNG.Utils
{
    public static class Extensions
    {
        public static IList<T> PushToTop<T>(this IList<T> list, Func<T, bool> pushPredicate)
        {
            return (list.Where(pushPredicate).Concat(list.Where(o => !pushPredicate(o)))).ToList();
        }
    }
}
