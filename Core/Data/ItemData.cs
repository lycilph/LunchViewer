using System;

namespace LunchViewerApp.Core.Data
{
    public sealed class ItemData
    {
        public string Id { get; set; }
        public Guid ParentId { get; set; }
        public Guid ItemId { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
    }
}
