using System;
using System.IO;
using System.Linq;
using System.Threading;
using GameScheduler.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace GameSchedulerMicroservice
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

            var configuration = builder.Build();

            //TODO: USE SECRETS!!!
            var mySettings = configuration.GetSection("MySettings");
            var messageBusConfiguration = configuration.GetSection("MessageBusConfiguration");

            var apiBaseUrl = mySettings.GetValue<string>("MYSPORTSFEEDS_BASEURL");
            var apiUsername = mySettings.GetValue<string>("MYSPORTSFEEDS_USERNAME");
            var apiPassword = mySettings.GetValue<string>("MYSPORTSFEEDS_PASSWORD");
            var seasonName = mySettings.GetValue<string>("SEASON_NAME");
            var connectionString = mySettings.GetValue<string>("CONNECTION_STRING");
            var databaseName =  mySettings.GetValue<string>("DATABASE_NAME");
            var collectionName = mySettings.GetValue<string>("COLLECTION_NAME");
            var format = mySettings.GetValue<string>("FORMAT");
            var gamScheduleUrl = mySettings.GetValue<string>("GAMESCHEDULE_URL");
            var interval = mySettings.GetValue<int>("API_REQUEST_INTERVAL_MS");

            var msgBusHost = messageBusConfiguration.GetValue<string>("Host");
            var msgBusUsername = messageBusConfiguration.GetValue<string>("Username");
            var msgBusPassword = messageBusConfiguration.GetValue<string>("Password");
            var msgBusReconnect = messageBusConfiguration.GetValue<int>("IntervalBetweenReconnectInSeconds");
            var msgBusConnectionName = messageBusConfiguration.GetValue<string>("ConnectionName");
            var msgBusExchange = messageBusConfiguration.GetValue<string>("Exchange");
            var msgBusQueue = messageBusConfiguration.GetValue<string>("Queue");
           
            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton< IGameScheduleRepository, GameScheduleRepository>(x => new GameScheduleRepository(connectionString, databaseName))
                .AddSingleton<IGameScheduleWebApiConsumer, GameScheduleWebApiConsumer>(x => new GameScheduleWebApiConsumer(new RestClient(apiBaseUrl) { Authenticator = new HttpBasicAuthenticator(apiUsername, apiPassword) }, gamScheduleUrl, format, seasonName))
                .AddSingleton<IMessageBusSetup, MessageBusSetup>(x => new MessageBusSetup(msgBusHost, msgBusUsername, msgBusPassword, msgBusReconnect, msgBusExchange, msgBusConnectionName, msgBusQueue, "direct"))
                .BuildServiceProvider();

            serviceProvider.GetService<ILoggerFactory>().AddConsole(LogLevel.Debug);

            //Setup MongoDB
            var repo = serviceProvider.GetService<IGameScheduleRepository>();
            var db = repo.Setup();
            var collection = db.GetCollection<BsonDocument>(collectionName);

            //Setup Web Api Consumer and Logging
            var gameScheduleWebApi = serviceProvider.GetService<IGameScheduleWebApiConsumer>();
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();

            // Get Response from Web API 
            logger.LogDebug("Calling Game Schedule Web API...");
            var gameScheduleResponse = gameScheduleWebApi.Get();

            //... store JSON to MongoDb database
            logger.LogDebug("Storing JSON to MongoDb database...");
            var jsonData = JsonConvert.SerializeObject(gameScheduleResponse);
            var document = BsonSerializer.Deserialize<dynamic>(jsonData);
            collection.InsertOne(document[0]);

            //Setup  RabbitMQ and publish message to the queue
            logger.LogDebug("Setting up RabbitMQ...");
            var publisher = serviceProvider.GetService<IMessageBusSetup>();

            //Find today's games and create list of games
            publisher.Publish("yo, it's working again");
            var result = 
            Console.ReadLine();
            logger.LogDebug("All done!");
        }
    }
}
