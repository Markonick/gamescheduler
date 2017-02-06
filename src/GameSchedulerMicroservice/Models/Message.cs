using System.Collections.Generic;

namespace GameSchedulerMicroservice.Models
{
    public class Message
    {
        public string Time { get; set; }
        public IEnumerable<string> GameId { get; set; }
    }
}