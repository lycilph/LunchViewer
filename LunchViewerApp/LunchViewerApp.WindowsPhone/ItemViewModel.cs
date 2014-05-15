using GalaSoft.MvvmLight;
using LunchViewerApp.Common;
using LunchViewerApp.Models;
using System;
using System.Windows.Input;
using Windows.System;

namespace LunchViewerApp
{
    public class ItemViewModel : ViewModelBase
    {
        private Item model;

        private MenuViewModel _Parent;
        public MenuViewModel Parent
        {
            get { return _Parent; }
        }

        public string Date
        {
            get { return model.Date.ToString("dd MMM yyyy"); }
        }

        public string Day
        {
            get { return model.Date.ToString("ddd"); }
        }

        public string Text
        {
            get { return model.Text; }
        }

        private bool _IsToday = false;
        public bool IsToday
        {
            get { return _IsToday; }
            set { Set(() => IsToday, ref _IsToday, value); }
        }

        public ICommand OrderCommand { get; private set; }

        public ItemViewModel(Item item, MenuViewModel parent)
        {
            model = item;
            _Parent = parent;

            OrderCommand = new RelayCommand(Order);
        }

        private async void Order()
        {
            await Launcher.LaunchUriAsync(new Uri(model.Link));
        }
    }
}
