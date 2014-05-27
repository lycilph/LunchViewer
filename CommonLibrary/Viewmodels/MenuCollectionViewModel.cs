using CommonLibrary.Utils;
using GalaSoft.MvvmLight;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CommonLibrary.Viewmodels
{
    public class MenuCollectionViewModel : ViewModelBase, IEnumerable<MenuViewModel>
    {
        public MenuViewModel Previous { get; set; }
        public MenuViewModel Current { get; set; }
        public MenuViewModel Next { get; set; }

        public MenuViewModel this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Previous;
                    case 1: return Current;
                    case 2: return Next;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public MenuCollectionViewModel()
        {
            Previous = new MenuViewModel("Previous (Week {0})");
            Current = new MenuViewModel("Current (Week {0})");
            Next = new MenuViewModel("Next (Week {0})");
        }

        public void Update()
        {
            var weeks = WeekUtils.Weeks;
            for (int i = 0; i < weeks.Count; i++)
                if (this[i].Week != weeks[i])
                    this[i].Read(weeks[i]);
        }

        public ItemViewModel GetNextItem()
        {
            ItemViewModel result = null;

            // Find next item (in current week)
            if (Current.HasItems)
            {
                var now = DateTime.Now;
                var lunch_end = new DateTime(now.Year, now.Month, now.Day, 13, 15, 0); // Lunch ends at 13:15

                if (now.CompareTo(lunch_end) < 0)
                    result = Current.Get(now);
                else
                    result = Current.Get(now.AddDays(1));
            }

            // If nothing was found choose the first item of the next week (if present)
            if (result == null && Next.HasItems)
                result = Next.Items.First();

            return result;
        }

        public IEnumerator<MenuViewModel> GetEnumerator()
        {
            yield return Previous;
            yield return Current;
            yield return Next;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
