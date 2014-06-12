using System;
using Windows.ApplicationModel.Resources;
using Core;

namespace LunchViewerApp.ViewModels
{
    public class NextItemViewModel : ObservableObject
    {
        private string _Header;
        public string Header
        {
            get { return _Header; }
            set { Set(ref _Header, value); }
        }

        private DateTime _Date;
        public DateTime Date
        {
            get { return _Date; }
            set { Set(ref _Date, value); }
        }

        private string _Text;
        public string Text
        {
            get { return _Text; }
            set { Set(ref _Text, value); }
        }

        private ItemViewModel _ItemViewModel;
        public ItemViewModel ItemViewModel
        {
            get { return _ItemViewModel; }
            set { Set(ref _ItemViewModel, value); }
        }

        public void Update(ItemViewModel item)
        {
            var loader = ResourceLoader.GetForViewIndependentUse();

            if (item == null)
                Header = loader.GetString("Unknown");
            else if (DateUtils.IsToday(item.Date))
                Header = loader.GetString("Today");
            else if (DateUtils.IsTomorrow(item.Date))
                Header = loader.GetString("Tomorrow");
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
                Text = loader.GetString("None");
            }
        }
    }
}
