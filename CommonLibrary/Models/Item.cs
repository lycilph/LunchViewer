using System;

namespace CommonLibrary.Models
{
    public sealed class Item
    {
        public string Id { get; set; }
        public Guid ParentId { get; set; }
        public Guid ItemId { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
    }
}
