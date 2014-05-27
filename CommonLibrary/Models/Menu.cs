using System;

namespace CommonLibrary.Models
{
    public sealed class Menu
    {
        public string Id { get; set; }
        public Guid MenuId { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }
    }
}
