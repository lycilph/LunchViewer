using Newtonsoft.Json;
using System;

namespace LunchViewerApp.Models
{
    public class Menu
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "MenuId")]
        public Guid MenuId { get; set; }

        [JsonProperty(PropertyName = "Year")]
        public int Year { get; set; }

        [JsonProperty(PropertyName = "Week")]
        public int Week { get; set; }
    }
}
