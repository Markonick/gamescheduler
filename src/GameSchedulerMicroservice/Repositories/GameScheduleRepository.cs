using MongoDB.Driver;

namespace GameScheduler.Repositories
{
    public class GameScheduleRepository : IGameScheduleRepository
    {
        private readonly string _connectionString;
        private readonly string _databaseName;

        public GameScheduleRepository(string connectionString, string databaseName)
        {
            _connectionString = connectionString;
            _databaseName = databaseName;
        }

        public IMongoDatabase  Setup()
        {
            var client = new MongoClient(_connectionString);
            return client.GetDatabase(_databaseName);
        }
    }
}
