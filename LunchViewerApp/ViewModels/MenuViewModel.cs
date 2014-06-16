using Core;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public void Read(int week_number)
        {
            if (week_number == model.Week)
                return;

            Header = string.Format(header_format, week_number);

            var week = MenuController.ReadMenu(week_number);
            if (week == null)
                return;

            model = week.Menu;
            Items = week.Items.Select(i => new ItemViewModel(i)).ToList();
        }
    }
}
