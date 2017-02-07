using System;
using System.Linq;
using GameSchedulerMicroservice.Helpers;
using GameSchedulerMicroservice.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace GameSchedulerMicroservice.Repositories
{
    public class GameScheduleRepository : IGameScheduleRepository
    {
        private readonly ILoggerFactory _logger;
        private readonly IMongoClient _client;
        private readonly string _databaseName;
        private readonly string _fullScheduleCollectionName;
        private readonly string _dailyScheduleCollectionName;
        private readonly ITimeProvider _timeProvider;

        public GameScheduleRepository(IMongoClient client, string databaseName, ILoggerFactory logger, ITimeProvider timeProvider, string fullScheduleCollectionName, string dailyScheduleCollectionName)
        {
            _client = client;
            _databaseName = databaseName;
            _timeProvider = timeProvider;
            _logger = logger;
            _fullScheduleCollectionName = fullScheduleCollectionName;
            _dailyScheduleCollectionName = dailyScheduleCollectionName;
        }

        public void StoreFullSchedule(dynamic response)
        {
            _logger.CreateLogger<Program>().LogDebug("Storing Full Schedule to MongoDb database...");
            var db = _client.GetDatabase(_databaseName);
            var collection = db.GetCollection<BsonDocument>(_fullScheduleCollectionName);
            var jsonData = JsonConvert.SerializeObject(response);
            var array = BsonSerializer.Deserialize<BsonArray>(jsonData);

            foreach (var document in array)
            {
                collection.InsertOne(document, null);
            }
        }

        public void StoreDailySchedule()
        {
            _logger.CreateLogger<Program>().LogDebug("Storing Daily Schedule to MongoDb database...");
            var db = _client.GetDatabase(_databaseName);
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var sourceCollection = db.GetCollection<BsonDocument>(_fullScheduleCollectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("date", today);
            var queryResult = sourceCollection.Find(filter).ToList();

            var targetCollection = db.GetCollection<BsonDocument>(_dailyScheduleCollectionName);

            foreach (var document in queryResult)
            {
                targetCollection.InsertOne(document, null);
            }
        }
        
        public Message GetNextGames()
        {
            var db = _client.GetDatabase(_databaseName);
            var collection = db.GetCollection<Game>(_dailyScheduleCollectionName);
            var now = _timeProvider.Time;
            var filter = Builders<Game>.Filter.Eq("time", now);
            var queryResult = collection.Find(filter).ToList();

            if (queryResult == null)
            {
                return null;
            }

            var message = new Message()
            {
                Time = now,
                GameId = queryResult
                        .Where(x => x.time == now)
                        .Select((x, y) => x.awayTeam.Abbreviation + "-" + x.homeTeam.Abbreviation)
            };

            return message;
        }
    }
}
