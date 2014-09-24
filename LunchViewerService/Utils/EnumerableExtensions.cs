using System;
using System.Collections.Generic;
using System.Linq;

namespace LunchViewerService.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Apply<T>(this IEnumerable<T> source, Action<T> action)
        {
            var items = source as IList<T> ?? source.ToList();

            foreach (var item in items)
                action(item);

            return items;
        }

        public static void ZipAndApply<T1, T2>(this IEnumerable<T1> list1, IEnumerable<T2> list2, Action<T1, T2> action)
        {
            list1.Zip(list2, (i1, i2) => new {i1, i2}).Apply(aggregate => action(aggregate.i1, aggregate.i2));
        }
    }
}
