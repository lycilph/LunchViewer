using CommonLibrary;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;

namespace LunchViewerApp.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private Menu model;
        private string header_format;

        private List<ItemViewModel> _Items;
        public List<ItemViewModel> Items
        {
            get { return _Items; }
            set { Set(() => Items, ref _Items, value); }
        }

        private string _Header;
        public string Header
        {
            get { return _Header; }
            set { Set(() => Header, ref _Header, value); }
        }

        public bool HasItems
        {
            get { return Items.Any(); }
        }

        public int Week
        {
            get { return model.Week; }
        }

        public MenuViewModel(string header_format)
        {
            model = new Menu();
            Items = new List<ItemViewModel>();

            this.header_format = header_format;
        }

        public ItemViewModel GetToday()
        {
            return Get(DateTime.Now.Date);
        }

        public ItemViewModel GetTomorrow()
        {
            return Get(DateTime.Now.Date.AddDays(1));
        }

        public ItemViewModel Get(DateTime date)
        {
            foreach (var item in Items)
            {
                if (item.Date == date)
                    return item;
            }

            return null;
        }

        public void Load(int week)
        {
            Header = string.Format(header_format, week);

            var menu_container = ApplicationData.Current.LocalSettings.CreateContainer("menus", ApplicationDataCreateDisposition.Always);
            var serialized_menu = menu_container.Values[week.ToString()];
            if (serialized_menu != null)
            {
                var type = new { Menu = new Menu(), Items = new List<Item>() };
                var obj = JsonConvert.DeserializeAnonymousType(serialized_menu as string, type);
                model = obj.Menu;
                Items = obj.Items.Select(i => new ItemViewModel(i)).ToList();

                RaisePropertyChanged("HasItems");
                RaisePropertyChanged("Week");
            }
        }
    }
}
