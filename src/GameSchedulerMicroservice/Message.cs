using System.Collections.Generic;

namespace GameScheduler.Repositories
{
    public class Message
    {
        public string Time { get; set; }
        public IEnumerable<string> GameId { get; set; }
    }
}