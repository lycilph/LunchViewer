using System;
using System.Linq;
using System.Collections.Generic;

namespace LunchViewerApp.Core
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Apply<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);

            return source;
        }

        public static IEnumerable<string> AsStrings<T>(this IEnumerable<T> source)
        {
            return source.Select(i => i.ToString());
        }
    }
}
