using System;

namespace LunchViewerApp.Core.Data
{
    public sealed class MenuData
    {
        public string Id { get; set; }
        public Guid MenuId { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }
    }
}
