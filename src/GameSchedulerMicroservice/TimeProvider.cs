using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameScheduler
{
    public class TimeProvider : ITimeProvider
    {
        public string Time { get; set; }

        public TimeProvider(string time)
        {
            Time = time;
        }
    }
}
