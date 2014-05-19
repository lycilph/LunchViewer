using CommonLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LunchViewerApp.Common;
using System;
using System.Windows.Input;
using Windows.System;

namespace LunchViewerApp.ViewModels
{
    public class ItemViewModel : ViewModelBase
    {
        private Item model;
        
        public DateTime Date
        {
            get { return model.Date; }
        }

        public string Text
        {
            get { return model.Text; }
        }

        private bool _IsNext;

        public bool IsNext
        {
            get { return _IsNext; }
            set { Set(() => IsNext, ref _IsNext, value); }
        }

        public ICommand OrderCommand { get; private set; }

        public ItemViewModel(Item item)
        {
            model = item;

            OrderCommand = new RelayCommand(Order);
        }

        public bool IsToday()
        {
            return DateTime.Now.Date.CompareTo(model.Date.Date) == 0;
        }

        public bool IsTomorrow()
        {
            return DateTime.Now.Date.AddDays(1).CompareTo(model.Date.Date) == 0;
        }

        private async void Order()
        {
            await Launcher.LaunchUriAsync(new Uri(model.Link));
        }
    }
}
