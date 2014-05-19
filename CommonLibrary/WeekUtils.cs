﻿using System;
using System.Globalization;

namespace CommonLibrary
{
    public static class WeekUtils
    {
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