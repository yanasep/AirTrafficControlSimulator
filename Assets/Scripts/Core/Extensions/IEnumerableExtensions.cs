using System;
using System.Collections.Generic;

namespace CatAdventure
{
    public static class IEnumerableExtensions
    {
        public static bool Any<TState, T>(this IEnumerable<T> self, TState state, Func<T, TState, bool> predicate)
        {
            foreach (var val in self)
            {
                if (predicate(val, state)) return true;
            }
            return false;
        }
    }
}