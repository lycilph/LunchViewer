using Core;
using LunchViewerApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;

namespace LunchViewerApp.ViewModels
{
    public class MenuViewModel : ObservableObject
    {
        private Menu model;
        private readonly string header_format;

        private string _Header;
        public string Header
        {
            get { return _Header; }
            set { Set(ref _Header, value); }
        }

        private List<ItemViewModel> _Items;
        public List<ItemViewModel> Items
        {
            get { return _Items; }
            private set
            {
                Set(ref _Items, value);
                RaisePropertyChanged("HasItems");
            }
        }

        public bool HasItems
        {
            get { return Items.Any(); }
        }

        public MenuViewModel(string header_format)
        {
            model = new Menu { Week = -1 };
            Items = new List<ItemViewModel>();

            this.header_format = header_format;
            Update(0);
        }

        public void Update(int week_number)
        {
            Header = string.Format(header_format, week_number);
        }

        public ItemViewModel Get(DateTime date)
        {
            return Items.FirstOrDefault(item => item.Date.Date == date.Date);
        }

        public void Read(int week)
        {
            if (week == model.Week)
                return;

            Header = string.Format(header_format, week);

            var menu_container = ApplicationData.Current.LocalSettings.CreateContainer("menus", ApplicationDataCreateDisposition.Always);
            var serialized_menu = menu_container.Values[week.ToString()] as string;
            if (serialized_menu == null) return;

            var type = new { Menu = new Menu(), Items = new List<Item>() };
            var obj = JsonConvert.DeserializeAnonymousType(serialized_menu, type);
            model = obj.Menu;
            Items = obj.Items.Select(i => new ItemViewModel(i)).ToList();
        }
    }
}
