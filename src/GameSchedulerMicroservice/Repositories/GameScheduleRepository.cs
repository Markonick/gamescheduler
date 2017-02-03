using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GameSchedulerMicroservice;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace GameScheduler.Repositories
{
    public class GameScheduleRepository : IGameScheduleRepository
    {
        private readonly ILoggerFactory _logger;
        private readonly IMongoClient _client;
        private readonly string _databaseName;
        private readonly string _fullScheduleCollectionName;
        private readonly string _dailyScheduleCollectionName;

        public GameScheduleRepository(IMongoClient client, string databaseName, ILoggerFactory logger, string fullScheduleCollectionName, string dailyScheduleCollectionName)
        {
            _client = client;
            _databaseName = databaseName;
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

        public Message GetNextGame(string inOneHour)
        {
            var db = _client.GetDatabase(_databaseName);
            var collection = db.GetCollection<Game>(_dailyScheduleCollectionName);
            var filter = Builders<Game>.Filter.Eq("time", inOneHour);
            var queryResult = collection.Find(filter).ToList().Where(x => x.time.Equals(inOneHour));

            /*if (queryResult != null)
            {
                var message = new Message()
                {
                    Time = inOneHour,
                    GameId = queryResult
                }
            }*/
            return null;
        }
    }
}
