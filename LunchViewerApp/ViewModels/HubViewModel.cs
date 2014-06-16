using Core;
using LunchViewerApp.Common;
using System;
using System.Linq;
using System.Windows.Input;

namespace LunchViewerApp.ViewModels
{
    public class HubViewModel : ObservableObject
    {
        public MenuCollectionViewModel Menus { get; private set; }
        public NextItemViewModel NextItem { get; private set; }

        public ICommand UpdateNowCommand { get; private set; }

        private bool _IsBusy;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set { Set(ref _IsBusy, value); }
        }

        public HubViewModel()
        {
            Menus = new MenuCollectionViewModel();
            NextItem = new NextItemViewModel();

            UpdateNowCommand = new RelayCommand(UpdateMenus);
        }

        public void LoadState()
        {
            Menus.ZipAndApply(DateUtils.Weeks, (model, week) => model.Read(week));
            UpdateNextItem();

            // If no menus were found, try to download
            if (Menus.All(m => !m.HasItems))
                UpdateMenus();
        }

        private async void UpdateMenus()
        {
            IsBusy = true;

            await MenuController.UpdateMenusAsync(App.MobileService);

            Menus.ZipAndApply(DateUtils.Weeks, (model, week) => model.Read(week));
            UpdateNextItem();

            IsBusy = false;
        }

        private void UpdateNextItem()
        {
            // Reset IsNext state
            Menus.Apply(m => m.Items.Apply(i => i.IsNext = false));

            // Mark item as next
            var item = FindNextItem();
            if (item != null)
                item.IsNext = true;

            // Hand over item to view model
            NextItem.Update(item);
        }

        private ItemViewModel FindNextItem()
        {
            ItemViewModel next_item_candidate = null;

            // Find next item (in current week)
            if (Menus.Current.HasItems)
                next_item_candidate = Menus.Current.Get(DateUtils.GetNextLunchDate());

            // If nothing was found choose the first item of the next week (if present)
            if (next_item_candidate == null && Menus.Next.HasItems)
                next_item_candidate = Menus.Next.Items.First();

            return next_item_candidate;
        }
    }
}
