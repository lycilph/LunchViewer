using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Tables;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Text;

namespace Models
{
    public class StorageData : TableEntity, ITableData
    {
        [IgnoreProperty]
        public string Id
        {
            get
            {
                return ((object)new CompositeTableKey(new string[2] {PartitionKey, RowKey})).ToString();
            }
            set
            {
                CompositeTableKey compositeTableKey;
                if (!CompositeTableKey.TryParse(value, out compositeTableKey) || compositeTableKey.Segments.Count != 2)
                {
                    PartitionKey = "Unknown";
                    RowKey = compositeTableKey.Segments[0];
                }
                else
                {
                    PartitionKey = compositeTableKey.Segments[0];
                    RowKey = compositeTableKey.Segments[1];
                }
            }
        }

        [IgnoreProperty]
        public DateTimeOffset? UpdatedAt
        {
            get
            {
                return new DateTimeOffset?(this.Timestamp);
            }
            set { Timestamp = value.HasValue ? value.Value : DateTimeOffset.UtcNow; }
        }

        [IgnoreProperty]
        public byte[] Version
        {
            get
            {
                if (this.ETag == null)
                    return (byte[])null;
                else
                    return Encoding.UTF8.GetBytes(this.ETag);
            }
            set
            {
                ETag = value != null ? Encoding.UTF8.GetString(value) : (string)null;
            }
        }

        public DateTimeOffset? CreatedAt { get; set; }

        public bool Deleted { get; set; }
    }
}
