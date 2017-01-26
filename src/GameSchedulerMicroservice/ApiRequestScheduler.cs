using System;
using System.Threading;
using System.Threading.Tasks;
using GameSchedulerMicroservice;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace GameSchedulerMicroservice
{
    public class ApiRequestScheduler
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IGameScheduleWebApiConsumer _gameSchedule;
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly int _requestInterval;

        public ApiRequestScheduler(IGameScheduleWebApiConsumer gameSchedule,  IMongoCollection<BsonDocument> collection, int requestInterval)
        {
            _gameSchedule = gameSchedule;
            _collection = collection;
            _requestInterval = requestInterval;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            var gameScheduleResponse = _gameSchedule.Get();
            var gameScheduleJson = gameScheduleResponse.Content;
            var document = BsonSerializer.Deserialize<BsonDocument>(gameScheduleJson);
            //document.Add("_id", _gameId);

            //_collection.DeleteOne(Builders<BsonDocument>.Filter.Eq("_id", _gameId));
            _collection.InsertOne(document);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        //Publish message informing subscribers that a game is starting...
                        //...some code...

                        Thread.Sleep(_requestInterval);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }, _cancellationTokenSource.Token);
        }
    }
}
