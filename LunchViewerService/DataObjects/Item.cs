using System;
using Microsoft.WindowsAzure.Mobile.Service;

namespace LunchViewerService.DataObjects
{
    public class Item : EntityData
    {
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
    }
}