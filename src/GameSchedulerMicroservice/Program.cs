using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoxScoreService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

            var appSettings = new ConfigurationSection().Section();

            var baseUrl = appSettings.GetValue<string>("MYSPORTSFEEDS_BASEURL");
            var username = appSettings.GetValue<string>("MYSPORTSFEEDS_USERNAME");
            var password = appSettings.GetValue<string>("MYSPORTSFEEDS_PASSWORD");
            var seasonName = appSettings.GetValue<string>("SEASON_NAME");
            var connectionString = appSettings.GetValue<string>("CONNECTION_STRING");
            var format = appSettings.GetValue<string>("FORMAT");
            var gamScheduleUrl = appSettings.GetValue<string>("GAMESCHEDULE_URL");
            var interval = appSettings.GetValue<int>("API_REQUEST_INTERVAL_MS");

            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IMongoDbSetup, MongoDbSetup>(x => new MongoDbSetup(connectionString))
                .AddSingleton<IGameScheduleWebApiConsumer, GameScheduleWebApiConsumer>(x => new GameScheduleWebApiConsumer(new RestClient(baseUrl) { Authenticator = new HttpBasicAuthenticator(username, password) }, gamScheduleUrl, format, seasonName))
                .BuildServiceProvider();

            serviceProvider.GetService<ILoggerFactory>().AddConsole(LogLevel.Debug);

            //Setup MongoDB
            var mongoDb = serviceProvider.GetService<IMongoDbSetup>();
            var collection = mongoDb.GetCollection();

            //Setup Web Api Consumer and Logging
            var gameScheduleWebApi = serviceProvider.GetService<IGameScheduleWebApiConsumer>();
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();
            gameScheduleWebApi.Get(); // Get schedule and 
            logger.LogDebug("Starting Game Scheduler application...");

            var scheduler = new ApiRequestScheduler(gameScheduleWebApi,  collection, interval);
            scheduler.Start();
            Task.Factory.StartNew(() => Console.WriteLine("hi from a shit thread"), cancellationTokenSource.Token);
            Console.ReadLine();

            logger.LogDebug("All done!");
        }
    }
}
