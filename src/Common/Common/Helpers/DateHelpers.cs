using System;

namespace Common.Helpers
{
    public static class DateHelpers
    {
        // https://stackoverflow.com/questions/4638993/difference-in-months-between-two-dates
        public static int DifferentInMonths(DateTime start, DateTime finish)
        {
            return ((finish.Year - start.Year) * 12) + finish.Month - start.Month;
            //return finish.Date.Subtract(start.Date).Days / (365.25 / 12);
        }

        public static double DifferentInDays(DateTime start, DateTime finish)
        {
            return (finish.Date - start.Date).TotalDays;
        }        
    }
}
