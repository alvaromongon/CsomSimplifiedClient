using Microsoft.Extensions.Configuration;
using Common.AppContext;

namespace Common.Configuration
{
    public static class ConfigurationRootFactory
    {
        /// <summary>
        /// Create a configuration root object
        /// </summary>
        /// <param name="applicationContext">application context</param>
        /// <returns><see cref="IConfigurationRoot"/> ready to be used</returns>
        public static IConfigurationRoot Create(IApplicationContext applicationContext)
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                 .SetBasePath(applicationContext.ExecutionPath)
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

            return configurationBuilder.Build();
        }
    }
}
