using System.Collections.Generic;

namespace Core.Models
{
    public class Week
    {
        public Menu Menu { get; set; }
        public List<Item> Items { get; set; }

        public Week(Menu menu, List<Item> items)
        {
            Menu = menu;
            Items = items;
        }
    }
}
