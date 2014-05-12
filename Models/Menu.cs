using System;
using System.Collections.Generic;

namespace Models
{
    public class Menu
    {
        public MenuEntity MenuEntity { get; private set; }
        public List<ItemEntity> ItemEntities { get; private set; }

        public Menu(int year, int week)
        {
            MenuEntity = new MenuEntity(year, week);
            ItemEntities = new List<ItemEntity>();
        }

        public void Add(DateTime date, string text)
        {
            ItemEntities.Add(new ItemEntity(date, text, MenuEntity.MenuId));
        }
    }
}
