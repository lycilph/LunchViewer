using System;
using System.Collections.Generic;
using System.Globalization;

namespace Core
{
    public static class DateUtils
    {
        public static bool IsToday(DateTime date)
        {
            return DateTime.Now.Date.CompareTo(date.Date) == 0;
        }

        public static bool IsTomorrow(DateTime date)
        {
            return DateTime.Now.AddDays(1).Date.CompareTo(date.Date) == 0;
        }

        public static DateTime GetNextLunchDate()
        {
            var now = DateTime.Now;
            var lunch_end = new DateTime(now.Year, now.Month, now.Day, 13, 15, 0); // Lunch ends at 13:15
            return (now.CompareTo(lunch_end) < 0 ? now : now.AddDays(1));
        }

        public static int PreviousWeekNumber
        {
            get { return GetPreviousWeekNumber(); }
        }

        public static int CurrentWeekNumber
        {
            get { return GetCurrentWeekNumber(); }
        }

        public static int NextWeekNumber
        {
            get { return GetNextWeekNumber(); }
        }

        public static List<int> Weeks
        {
            get { return new List<int> { PreviousWeekNumber, CurrentWeekNumber, NextWeekNumber }; }
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

        public static int WeekOfYearISO8601(DateTime date)
        {
            var day = (int)CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(date);
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date.AddDays(4 - (day == 0 ? 7 : day)), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}