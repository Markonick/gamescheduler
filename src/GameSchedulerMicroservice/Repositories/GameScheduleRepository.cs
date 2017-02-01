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
        private readonly object _response;
        private readonly ILoggerFactory _logger;
        private readonly string _fullCollectionName;

        public IMongoDatabase Db { get; set; }

        public GameScheduleRepository(IMongoClient client, string databaseName, dynamic response, ILoggerFactory logger, string fullCollectionName)
        {
            _response = response;
            _logger = logger;
            _fullCollectionName = fullCollectionName;

            Db = client.GetDatabase(databaseName);
        }

        public void StoreFullSchedule()
        {
            _logger.CreateLogger<Program>().LogDebug("Storing Full Schedule to MongoDb database...");

            var collection = Db.GetCollection<BsonDocument>(_fullCollectionName);
            var jsonData = JsonConvert.SerializeObject(_response);

            var array = BsonSerializer.Deserialize<BsonArray>(jsonData);
            foreach (var document in array)
            {
                collection.InsertOne(document, null, CancellationToken.None);
            }
        }
        
        public void StoreDailySchedule()
        {
            _logger.CreateLogger<Program>().LogDebug("Storing Daily Schedule to MongoDb database...");

            var collection = Db.GetCollection<BsonDocument>(_fullCollectionName);
            var jsonData = JsonConvert.SerializeObject(_response);

            var array = BsonSerializer.Deserialize<BsonArray>(jsonData);
            foreach (var document in array)
            {
                collection.InsertOne(document);
            }
        }
    }
}
