using MongoDB.Bson;
using MongoDB.Driver;

namespace GameSchedulerMicroservice
{
    public interface IMongoDbSetup
    {
        IMongoCollection<BsonDocument> GetCollection();
    }

    public class MongoDbSetup : IMongoDbSetup
    {
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly string _collectionName;

        public MongoDbSetup(string connectionString, string databaseName, string collectionName)
        {
            _connectionString = connectionString;
            _databaseName = databaseName;
            _collectionName = collectionName;
        }

        public IMongoCollection<BsonDocument> GetCollection()
        {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase(_databaseName);
            var collection = db.GetCollection<BsonDocument>(_collectionName);

            return collection;
        }
    }
}