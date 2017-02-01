using System;
using System.Threading;
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

        public GameScheduleRepository(IMongoClient client, string databaseName, ILoggerFactory logger)
        {
            _client = client;
            _databaseName = databaseName;
            _logger = logger;
        }

        public void StoreFullSchedule(dynamic response, string collectionName)
        {
            _logger.CreateLogger<Program>().LogDebug("Storing Full Schedule to MongoDb database...");
            var db = _client.GetDatabase(_databaseName);
            var collection = db.GetCollection<BsonDocument>(collectionName);
            var jsonData = JsonConvert.SerializeObject(response);
            var array = BsonSerializer.Deserialize<BsonArray>(jsonData);

            foreach (var document in array)
            {
                collection.InsertOne(document, null);
            }
        }
        
        public void StoreDailySchedule(string sourceName, string targetName)
        {
            _logger.CreateLogger<Program>().LogDebug("Storing Daily Schedule to MongoDb database...");
            var db = _client.GetDatabase(_databaseName);
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var sourceCollection = db.GetCollection<BsonDocument>(sourceName);
            var filter = Builders<BsonDocument>.Filter.Eq("date", today);
            var queryResult = sourceCollection.Find(filter).ToList();

            var targetCollection = db.GetCollection<BsonDocument>(targetName);
            //Create daily schedule collection and add to db
            foreach (var document in queryResult)
            {
                targetCollection.InsertOne(document, null);
            }
        }
    }
}
