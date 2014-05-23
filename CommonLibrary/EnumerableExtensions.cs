using System;
using System.Collections.Generic;

namespace CommonLibrary
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Apply<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);

            return source;
        }
    }
}
