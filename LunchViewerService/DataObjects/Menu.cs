using System.Collections.Generic;
using Microsoft.WindowsAzure.Mobile.Service;

namespace LunchViewerService.DataObjects
{
    public class Menu : EntityData
    {
        public int Year { get; set; }
        public int Week { get; set; }

        public ICollection<Item> Items { get; set; }
    }
}