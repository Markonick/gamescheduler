using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Exceptions;
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
                .AddSingleton<IMongoDbSetup, MongoDbSetup>(x => new MongoDbSetup(connectionString, databaseName, collectionName))
                .AddSingleton<IGameScheduleWebApiConsumer, GameScheduleWebApiConsumer>(x => new GameScheduleWebApiConsumer(new RestClient(apiBaseUrl) { Authenticator = new HttpBasicAuthenticator(apiUsername, apiPassword) }, gamScheduleUrl, format, seasonName))
                .AddSingleton<IMessageSetup, MessageBusSetup>(x => new MessageBusSetup(msgBusHost, msgBusUsername, msgBusPassword, msgBusReconnect, msgBusExchange, msgBusConnectionName, msgBusQueue, "direct"))
                .BuildServiceProvider();

            serviceProvider.GetService<ILoggerFactory>().AddConsole(LogLevel.Debug);

            //Setup MongoDB
            var mongoDb = serviceProvider.GetService<IMongoDbSetup>();
            var collection = mongoDb.GetCollection();

            //Setup Web Api Consumer and Logging
            var gameScheduleWebApi = serviceProvider.GetService<IGameScheduleWebApiConsumer>();
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();
            gameScheduleWebApi.Get(); // Get schedule and store in database
            logger.LogDebug("Starting Game Scheduler application...");
            
            var publisher = serviceProvider.GetService<IMessageSetup>();
            var channel = publisher.Setup();

            //scheduler.Start();
            Task.Factory.StartNew(() => Console.WriteLine("hi from a shit thread"), cancellationTokenSource.Token);
            Console.ReadLine();

            logger.LogDebug("All done!");
        }
    }
}
