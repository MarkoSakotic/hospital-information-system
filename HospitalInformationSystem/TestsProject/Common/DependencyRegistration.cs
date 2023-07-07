using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RepositoryProject.Context;
using System.IO;
using System.Text;

namespace TestsProject.Common
{
    public static class TestingConfigurationBuilder
    {
        public static IConfigurationRoot BuildConfiguration()
        {
            var testingConfiguration = GetTestingConfiguration();

            var serialized = JsonConvert.SerializeObject(testingConfiguration);
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(serialized)));
            return configurationBuilder.Build();
        }

        public static IConfiguration GetTestConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.test.json", true, true)
                .Build();
            return config;
        }

        private static JsonClientTests.TestingConfiguration GetTestingConfiguration()
        {
            return new JsonClientTests.TestingConfiguration
            {
                HisConfiguration = new HisConfiguration
                {
                    HisConnection = "asd"
                }
            };
        }
    }

}
