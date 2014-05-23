using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LunchViewerApp.Common;
using System;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using CommonLibrary;

namespace LunchViewerApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private List<MenuViewModel> week_menus;

        public NextItemViewModel NextItem { get; set; }
        public MenuViewModel PreviousWeekMenu { get; set; }
        public MenuViewModel CurrentWeekMenu { get; set; }
        public MenuViewModel NextWeekMenu { get; set; }

        private ItemViewModel _Today;
        public ItemViewModel Today
        {
            get { return _Today; }
            set { Set(() => _Today, ref _Today, value); }
        }

        private bool _IsBusy = false;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set { Set(() => IsBusy, ref _IsBusy, value); }
        }

        public ICommand HomeCommand { get; set; }
        public ICommand UpdateNowCommand { get; set; }
        public ICommand ShowLogCommand { get; set; }

        public MainViewModel()
        {
            PreviousWeekMenu = new MenuViewModel("Previous (Week {0})");
            CurrentWeekMenu = new MenuViewModel("Current (Week {0})");
            NextWeekMenu = new MenuViewModel("Next (Week {0})");
            week_menus = new List<MenuViewModel> { PreviousWeekMenu, CurrentWeekMenu, NextWeekMenu };

            NextItem = new NextItemViewModel();

            UpdateNowCommand = new RelayCommand(UpdateNow);
            ShowLogCommand = new RelayCommand(ShowLog);
        }

        private async void UpdateNow()
        {
            IsBusy = true;

            await MenuDownloader.Execute(App.MobileService);
            LoadMenus();

            IsBusy = false;
        }

        private void ShowLog()
        {
            
        }

        private void LoadMenus()
        {
            var weeks = new List<int> { WeekUtils.PreviousWeekNumber, WeekUtils.CurrentWeekNumber, WeekUtils.NextWeekNumber };
            for (int i = 0; i < weeks.Count; i++)
                if (week_menus[i].Week != weeks[i])
                    week_menus[i].Load(weeks[i]);

            UpdateNextItem();
        }

        private void UpdateNextItem()
        {
            ItemViewModel next_item = null;

            // Reset IsNext state
            foreach (var menu in week_menus)
                menu.Items.Apply(i => i.IsNext = false);

            // Find next item (in current week)
            if (CurrentWeekMenu.HasItems)
            {
                var now = DateTime.Now;
                var lunch_end = new DateTime(now.Year, now.Month, now.Day, 13, 15, 0); // Lunch ends at 13:15

                if (now.CompareTo(lunch_end) < 0)
                    next_item = CurrentWeekMenu.GetToday();
                else
                    next_item = CurrentWeekMenu.GetTomorrow();
            }

            // If nothing was found choose the first item of the next week (if present)
            if (next_item == null && NextWeekMenu.HasItems)
            {
                next_item = NextWeekMenu.Items.First();
            }

            // Update the next item
            NextItem.Update(next_item);
            if (next_item != null)
                next_item.IsNext = true;
        }

        public void LoadState()
        {
            LoadMenus();

            var has_no_menus = week_menus.All(m => !m.HasItems);
            if (has_no_menus)
                UpdateNow();
        }
    }
}
