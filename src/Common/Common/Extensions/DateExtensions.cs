using System;
using System.Globalization;

namespace Common.Extensions
{
    public static class DateExtensions
    {
        public static DateTime FirstDayOfMonth(this DateTime value)
        {
            return value.Date.AddDays(1 - value.Day);
        }

        public static DateTime LastDayOfMonth(this DateTime value)
        {
            return value.AddDays(DateTime.DaysInMonth(value.Year, value.Month) - 1);
        }

        public static string MonthName(this DateTime value, CultureInfo cultureInfo)
        {
            return value.ToString("MMMM", cultureInfo);
        }
    }
}
