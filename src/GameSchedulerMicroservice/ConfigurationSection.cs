using System.IO;
using Microsoft.Extensions.Configuration;

namespace GameSchedulerMicroservice
{
    public class ConfigurationSection
    {
        public IConfigurationSection Section()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();
            //TODO: USE SECRETS!!!
            var appSettings = configuration.GetSection("MySettings");

            return appSettings;
        }
    }
}