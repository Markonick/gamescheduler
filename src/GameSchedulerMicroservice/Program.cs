using System;
using System.IO;
using System.Threading.Tasks;
using GameSchedulerMicroservice.Helpers;
using GameSchedulerMicroservice.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using RestSharp;
using RestSharp.Authenticators;

namespace GameSchedulerMicroservice
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            //setup our DI
            var serviceProvider = ConfigureServices();

            //Setup MongoDB Web Api Consumer and Logging
            var repo = serviceProvider.GetService<IGameScheduleRepository>();
            var gameScheduleWebApi = serviceProvider.GetService<IGameScheduleWebApiConsumer>();
            serviceProvider.GetService<ILoggerFactory>().AddConsole(LogLevel.Debug);
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();

            // Get Response from Web API at service startup and store full schedule 
            logger.LogDebug("Calling Game Schedule Web API...");
            var gameScheduleResponse = gameScheduleWebApi.Get();
            repo.StoreFullSchedule(gameScheduleResponse);

            //Start daily jobs: 1) Store daily games, 2) Poll for games and publsih messages if upcoming games about to start
            var scheduler = new MyScheduler(repo);
            Task.Factory.StartNew(async () => await scheduler.Start());
            //TODO

            logger.LogDebug("All done!");
            Console.ReadLine();
        }

        private static IServiceProvider ConfigureServices()
        {
            IConfigurationRoot configuration = GetConfiguration();

            IServiceCollection services = new ServiceCollection();

            var mySettings = configuration.GetSection("MySettings");
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

            var loggerFactory = new LoggerFactory()
                .AddConsole();

            services.AddSingleton(loggerFactory);
            services.AddLogging();

            services.AddSingleton<IGameScheduleRepository, GameScheduleRepository>(x =>new GameScheduleRepository(new MongoClient(connectionString), databaseName, new LoggerFactory(), 
                new TimeProvider(DateTime.UtcNow.AddHours(0).ToString("HH:mmtt")), fullScheduleCollectionName, dailyScheduleCollectionName));

            services.AddSingleton<IGameScheduleWebApiConsumer, GameScheduleWebApiConsumer>(x =>new GameScheduleWebApiConsumer(
                new RestClient(apiBaseUrl) {Authenticator = new HttpBasicAuthenticator(apiUsername, apiPassword)}, gamScheduleUrl, format, seasonName));

            services.AddSingleton<IMessageBusSetup, MessageBusSetup>(x => new MessageBusSetup(
                msgBusHost, msgBusUsername, msgBusPassword, msgBusReconnect, msgBusExchange, msgBusConnectionName, msgBusQueue, "direct"));
            services.AddSingleton<ITimeProvider, TimeProvider>();
            services.AddSingleton<IJob, StoreDailyGamesJob>();
            return services.BuildServiceProvider();
        }

        private static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json").Build();
        }
    }
}
