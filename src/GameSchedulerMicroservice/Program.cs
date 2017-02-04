using System;
using System.IO;
using System.Threading;
using GameScheduler;
using GameScheduler.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Quartz;
using RestSharp;
using RestSharp.Authenticators;

namespace GameSchedulerMicroservice
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
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
            var fullScheduleCollectionName = mySettings.GetValue<string>("FULL_SCHEDULE_COLLECTION_NAME");
            var dailyScheduleCollectionName = mySettings.GetValue<string>("DAILY_SCHEDULE_COLLECTION_NAME");
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
                .AddSingleton<IGameScheduleRepository, GameScheduleRepository>(x => new GameScheduleRepository(new MongoClient(connectionString), databaseName,  new LoggerFactory(), fullScheduleCollectionName, dailyScheduleCollectionName))
                .AddSingleton<IGameScheduleWebApiConsumer, GameScheduleWebApiConsumer>(x => new GameScheduleWebApiConsumer(new RestClient(apiBaseUrl) { Authenticator = new HttpBasicAuthenticator(apiUsername, apiPassword) }, gamScheduleUrl, format, seasonName))
                .AddSingleton<IMessageBusSetup, MessageBusSetup>(x => new MessageBusSetup(msgBusHost, msgBusUsername, msgBusPassword, msgBusReconnect, msgBusExchange, msgBusConnectionName, msgBusQueue, "direct"))
                .BuildServiceProvider();

            serviceProvider.GetService<ILoggerFactory>().AddConsole(LogLevel.Debug);

            //Setup MongoDB
            var repo = serviceProvider.GetService<IGameScheduleRepository>();

            //Setup Web Api Consumer and Logging
            var gameScheduleWebApi = serviceProvider.GetService<IGameScheduleWebApiConsumer>();
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();

            // Get Response from Web API 
            logger.LogDebug("Calling Game Schedule Web API...");
            var gameScheduleResponse = gameScheduleWebApi.Get();
            
            //Store full schedule at service startup
            repo.StoreFullSchedule(gameScheduleResponse);

            //Store daily schedule at 00:00 ET every day
            

            //This is where we loop, checking for upcoming games continuously. We create messages upcoming games
            //to be consumed by subscribers

            var time = DateTime.UtcNow.AddHours(0).ToString("HH:mmtt");
            var msg = repo.GetNextGames(time);

            //Setup  RabbitMQ and publish message to the queue
            logger.LogDebug("Setting up RabbitMQ...");
            var publisher = serviceProvider.GetService<IMessageBusSetup>();

            //Find today's games and create list of games
            publisher.Publish(msg);
            //var result = 
            logger.LogDebug("All done!");
            Console.ReadLine();
        }
    }
}
