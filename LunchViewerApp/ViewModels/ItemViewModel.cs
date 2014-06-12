using Core;
using LunchViewerApp.Common;
using LunchViewerApp.Models;
using System;
using System.Windows.Input;
using Windows.System;

namespace LunchViewerApp.ViewModels
{
    public class ItemViewModel : ObservableObject
    {
        private readonly Item model;

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

        private async void Order()
        {
            await Launcher.LaunchUriAsync(new Uri(model.Link));
        }
    }
}
