using System;
using System.Globalization;

namespace LunchViewerApp
{
    public static class WeekUtils
    {
        public static int PreviousWeekNumber
        {
            get { return 18; } // GetPreviousWeekNumber(); }
        }

        public static int CurrentWeekNumber
        {
            get { return 19; } // GetCurrentWeekNumber(); }
        }

        public static int NextWeekNumber
        {
            get { return 20; } // GetNextWeekNumber(); }
        }

        public static int WeekOfYearISO8601(DateTime date)
        {
            var day = (int)CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(date);
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date.AddDays(4 - (day == 0 ? 7 : day)), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static int GetPreviousWeekNumber()
        {
            return WeekOfYearISO8601(DateTime.Now.AddDays(-7));
        }

        public static int GetCurrentWeekNumber()
        {
            return WeekOfYearISO8601(DateTime.Now);
        }

        public static int GetNextWeekNumber()
        {
            return WeekOfYearISO8601(DateTime.Now.AddDays(7));
        }
    }
}
