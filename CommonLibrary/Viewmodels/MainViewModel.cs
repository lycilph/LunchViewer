using CommonLibrary;
using CommonLibrary.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Linq;
using System.Windows.Input;

namespace CommonLibrary.Viewmodels
{
    public class MainViewModel : ViewModelBase
    {
        public MenuCollectionViewModel Menus { get; set; }
        public NextItemViewModel NextItem { get; set; }

        private bool _IsBusy = false;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set { Set(() => IsBusy, ref _IsBusy, value); }
        }

        public ICommand UpdateNowCommand { get; set; }

        public MainViewModel()
        {
            Menus = new MenuCollectionViewModel();
            NextItem = new NextItemViewModel();

            UpdateNowCommand = new RelayCommand(UpdateNow);
        }

        public void LoadState()
        {
            Menus.Update();
            UpdateNextItem();

            // If no menus were found, try to download
            if (Menus.All(m => !m.HasItems))
                UpdateNow();
        }

        private async void UpdateNow()
        {
            IsBusy = true;

            await DownloadUtils.DownloadMenusAsync(MobileServiceUtils.CreateMobileServiceClient());
            Menus.Update();
            UpdateNextItem();

            IsBusy = false;
        }

        private void UpdateNextItem()
        {
            var item = Menus.GetNextItem();

            // Reset IsNext state and set new one
            Menus.Apply(m => m.Items.Apply(i => i.IsNext = false));
            if (item != null)
                item.IsNext = true;

            // Hand over item to view model
            NextItem.Update(item);
        }
    }
}
