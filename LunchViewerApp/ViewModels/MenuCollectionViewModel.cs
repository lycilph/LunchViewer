using System;
using System.Collections;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;

namespace LunchViewerApp.ViewModels
{
    public class MenuCollectionViewModel : ObservableObject, IEnumerable<MenuViewModel>
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
            var loader = ResourceLoader.GetForViewIndependentUse();

            Previous = new MenuViewModel(loader.GetString("PreviousWeekHeader"));
            Current = new MenuViewModel(loader.GetString("CurrentWeekHeader"));
            Next = new MenuViewModel(loader.GetString("NextWeekHeader"));
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
