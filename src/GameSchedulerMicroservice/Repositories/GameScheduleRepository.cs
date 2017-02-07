using System;
using System.Collections;
using System.Collections.Generic;
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
            var today = _timeProvider.Date;
            var sourceCollection = db.GetCollection<BsonDocument>(_fullScheduleCollectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("date", today);
            var queryResult = sourceCollection.Find(filter).ToList();

            var targetCollection = db.GetCollection<BsonDocument>(_dailyScheduleCollectionName);

            foreach (var document in queryResult)
            {
                targetCollection.InsertOne(document, null);
            }
        }
        
        public IList<Message> GetNextGames()
        {
            var db = _client.GetDatabase(_databaseName);
            var collection = db.GetCollection<Game>(_dailyScheduleCollectionName);
            var today = _timeProvider.Date;
            var filter = Builders<Game>.Filter.Eq("date", today);
            var queryResult = collection.Find(filter).ToList();
            var message = new List<Message>();

            if (queryResult == null)
            {
                return null;
            }

            foreach (var elem in queryResult)
            {
                message.Add(new Message()
                {
                    Time = elem.time,
                    GameId = elem.awayTeam.Abbreviation + "-" + elem.homeTeam.Abbreviation
                });
            }

            return message;
        }
    }
}
