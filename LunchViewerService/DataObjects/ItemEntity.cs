using System;
using System.Xml;

namespace LunchViewerService.DataObjects
{
    public class ItemEntity : StorageData
    {
        public Guid ItemId { get; set; }
        public Guid ParentId { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }

        public ItemEntity() { }
        public ItemEntity(DateTime date, string text, string link, Guid parent)
        {
            ItemId = Guid.NewGuid();
            ParentId = parent;
            Date = date;
            Text = text;
            Link = link;

            PartitionKey = ParentId.ToString();
            RowKey = XmlConvert.ToString(date, XmlDateTimeSerializationMode.RoundtripKind);
        }
    }
}
