using System;

namespace BoxScoreService
{
    public interface IDateProvider
    {
        string Date { get; set; }
    }
}