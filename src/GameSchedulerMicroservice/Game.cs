using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameSchedulerMicroservice
{
    public class Game
    {
        public ObjectId Id { get; set; }
        public string id { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public Team awayTeam { get; set; }
        public Team homeTeam { get; set; }
        public string location { get; set; }
    }
}