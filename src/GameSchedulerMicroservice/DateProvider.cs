using System;
using System.Dynamic;

namespace BoxScoreService
{
    public class DateProvider : IDateProvider
    {
        public string Date { get; set; }

        public DateProvider(string date)
        {
            Date = date;
        }
    }
}