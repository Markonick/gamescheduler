using System;
using System.IO;
using System.Threading;
using GameScheduler;
using GameScheduler.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Quartz;
using Quartz.Impl;
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

            

            //setup our DI
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

                .AddLogging()
                .AddSingleton<IGameScheduleRepository, GameScheduleRepository>(x => new GameScheduleRepository(new MongoClient(connectionString), databaseName,  new LoggerFactory(), new TimeProvider(DateTime.UtcNow.AddHours(0).ToString("HH:mmtt")), fullScheduleCollectionName, dailyScheduleCollectionName))
                .AddSingleton<IGameScheduleWebApiConsumer, GameScheduleWebApiConsumer>(x => new GameScheduleWebApiConsumer(new RestClient(apiBaseUrl) { Authenticator = new HttpBasicAuthenticator(apiUsername, apiPassword) }, gamScheduleUrl, format, seasonName))
                .AddSingleton<IMessageBusSetup, MessageBusSetup>(x => new MessageBusSetup(msgBusHost, msgBusUsername, msgBusPassword, msgBusReconnect, msgBusExchange, msgBusConnectionName, msgBusQueue, "direct"))
                //.AddSingleton<IJob, StoreDailyGamesJob>(x => new StoreDailyGamesJob(gameRepo))
                .BuildServiceProvider();
            serviceProvider.
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

            //Start daily jobs: 1) Store daily games, 2) Poll for games and publsih messages if upcoming games about to start
            var sched = new JobScheduler().Start();

            var time = DateTime.UtcNow.AddHours(0).ToString("HH:mmtt");

            //Setup  RabbitMQ and publish message to the queue
            logger.LogDebug("Setting up RabbitMQ...");
            var publisher = serviceProvider.GetService<IMessageBusSetup>();

            logger.LogDebug("All done!");
            Console.ReadLine();
        }

        private static void ConfigureServices(IServiceCollection services)
        {

            var loggerFactory = new LoggerFactory()
                .AddConsole();

            services.AddSingleton(loggerFactory);
            services.AddLogging();
            IConfigurationRoot configuration = GetConfiguration();
        }

        private static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", optional: true)
                .Build();//TODO: USE SECRETS!!!

            var mySettings = GetSection("MySettings");
            var apiBaseUrl = mySettings.GetValue<string>("MYSPORTSFEEDS_BASEURL");
            var apiUsername = mySettings.GetValue<string>("MYSPORTSFEEDS_USERNAME");
            var apiPassword = mySettings.GetValue<string>("MYSPORTSFEEDS_PASSWORD");
            var seasonName = mySettings.GetValue<string>("SEASON_NAME");
            var connectionString = mySettings.GetValue<string>("CONNECTION_STRING");
            var databaseName = mySettings.GetValue<string>("DATABASE_NAME");
            var fullScheduleCollectionName = mySettings.GetValue<string>("FULL_SCHEDULE_COLLECTION_NAME");
            var dailyScheduleCollectionName = mySettings.GetValue<string>("DAILY_SCHEDULE_COLLECTION_NAME");
            var format = mySettings.GetValue<string>("FORMAT");
            var gamScheduleUrl = mySettings.GetValue<string>("GAMESCHEDULE_URL");
            var interval = mySettings.GetValue<int>("API_REQUEST_INTERVAL_MS");

            var messageBusConfiguration = configuration.GetSection("MessageBusConfiguration");
            var msgBusHost = messageBusConfiguration.GetValue<string>("Host");
            var msgBusUsername = messageBusConfiguration.GetValue<string>("Username");
            var msgBusPassword = messageBusConfiguration.GetValue<string>("Password");
            var msgBusReconnect = messageBusConfiguration.GetValue<int>("IntervalBetweenReconnectInSeconds");
            var msgBusConnectionName = messageBusConfiguration.GetValue<string>("ConnectionName");
            var msgBusExchange = messageBusConfiguration.GetValue<string>("Exchange");
            var msgBusQueue = messageBusConfiguration.GetValue<string>("Queue");
        }
    }
}
