using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunchViewerApp.Models
{
    public class Item
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "ParentId")]
        public Guid ParentId { get; set; }

        [JsonProperty(PropertyName = "ItemId")]
        public Guid ItemId { get; set; }

        [JsonProperty(PropertyName = "Date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "Text")]
        public string Text { get; set; }
    }
}
