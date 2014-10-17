using System;
using System.Collections.Generic;

namespace MailWorkerRole.Utils
{
    public static class EnumerableExtensions
    {
        public static void Apply<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var element in source)
                action(element);
        }
    }
}
