using System;
using System.Globalization;

namespace Models
{
    public class MenuEntity : StorageData
    {
        public Guid MenuId { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }

        public MenuEntity() { }
        public MenuEntity(int year, int week)
        {
            MenuId = Guid.NewGuid();
            Year = year;
            Week = week;

            PartitionKey = Year.ToString(CultureInfo.InvariantCulture);
            RowKey = Week.ToString(CultureInfo.InvariantCulture);
        }
    }
}
