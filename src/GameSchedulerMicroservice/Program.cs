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
using RestSharp;
using RestSharp.Authenticators;

namespace GameSchedulerMicroservice
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            //Setup our DI container
            var serviceProvider = ConfigureServices();

            //Setup MongoDB Web Api Consumer and Logging
            var repo = serviceProvider.GetService<IGameScheduleRepository>();
            var mesgBus = serviceProvider.GetService<IMessageBusSetup>();
            var gameScheduleWebApi = serviceProvider.GetService<IGameScheduleWebApiConsumer>();
            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();

            //Get Response from Web API at service startup and store full schedule 
            logger.LogDebug("Calling Game Schedule Web API...");
            var gameScheduleResponse = gameScheduleWebApi.Get();
            repo.StoreFullSchedule(gameScheduleResponse);

            //Start daily jobs: 1) Store daily games, 2) Poll for games and publsih messages if upcoming games about to start
            var scheduler = new DailyJobScheduler(repo, mesgBus, logger);
            Task.Factory.StartNew(async () => await scheduler.Start());
            //TODO

            logger.LogDebug("All done!");
            Console.ReadLine();
        }

        private static IServiceProvider ConfigureServices()
        {
            var configuration = GetConfiguration();

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
            var exchangeType = messageBusConfiguration.GetValue<string>("ExchangeType");

            var loggerFactory = new LoggerFactory()
                .AddConsole(LogLevel.Debug);

            services.AddSingleton(loggerFactory);
            services.AddLogging();

            var date = DateTime.Today.ToString("yyyy-MM-dd");
            services.AddSingleton<IGameScheduleRepository, GameScheduleRepository>(x =>new GameScheduleRepository(new MongoClient(connectionString), databaseName, loggerFactory, 
                new TimeProvider(date), fullScheduleCollectionName, dailyScheduleCollectionName));

            services.AddSingleton<IGameScheduleWebApiConsumer, GameScheduleWebApiConsumer>(x =>new GameScheduleWebApiConsumer(
                new RestClient(apiBaseUrl) {Authenticator = new HttpBasicAuthenticator(apiUsername, apiPassword)}, gamScheduleUrl, format, seasonName));

            services.AddSingleton<IMessageBusSetup, MessageBusSetup>(x => new MessageBusSetup(
                msgBusHost, msgBusUsername, msgBusPassword, msgBusReconnect, msgBusExchange, msgBusConnectionName, msgBusQueue, exchangeType));

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
