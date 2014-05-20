using GalaSoft.MvvmLight;
using System;

namespace LunchViewerApp.ViewModels
{
    public class NextItemViewModel : ViewModelBase
    {
        private string _Header;
        public string Header
        {
            get { return _Header; }
            set { Set(() => Header, ref _Header, value); }
        }

        private DateTime _Date;
        public DateTime Date
        {
            get { return _Date; }
            set { Set(() => Date, ref _Date, value); }
        }

        private string _Text;
        public string Text
        {
            get { return _Text; }
            set { Set(() => Text, ref _Text, value); }
        }

        private ItemViewModel _ItemViewModel;
        public ItemViewModel ItemViewModel
        {
            get { return _ItemViewModel; }
            set { Set(() => ItemViewModel, ref _ItemViewModel, value); }
        }
        
        public void Update(ItemViewModel item)
        {
            if (item == null)
            {
                Header = "Next lunch";
            }
            else if (item.IsToday())
                Header = "Today";
            else if (item.IsTomorrow())
                Header = "Tomorrow";
            else
                Header = item.Date.ToString("dddd");

            ItemViewModel = item;
            if (item != null)
            {
                Date = item.Date;
                Text = item.Text;
            }
            else
            {
                Date = DateTime.Now;
                Text = "None";
            }
        }
    }
}
