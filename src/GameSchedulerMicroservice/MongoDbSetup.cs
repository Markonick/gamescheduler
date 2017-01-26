using MongoDB.Bson;
using MongoDB.Driver;

namespace BoxScoreService
{
    public interface IMongoDbSetup
    {
        IMongoCollection<BsonDocument> GetCollection();
    }

    public class MongoDbSetup : IMongoDbSetup
    {
        private readonly string _connectionString;

        public MongoDbSetup(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IMongoCollection<BsonDocument> GetCollection()
        {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase("BoxScoreDatabase");
            var collection = db.GetCollection<BsonDocument>("BoxScoreCollection");

            return collection;
        }
    }
}