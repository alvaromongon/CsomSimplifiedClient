using Microsoft.Extensions.Configuration;

namespace Common.Configuration
{
    public class ConfigurationGetter : IConfigurationGetter
    {
        public readonly IConfigurationRoot _configurationRoot;

        public ConfigurationGetter(IConfigurationRoot configurationRoot)
        {
            _configurationRoot = configurationRoot;
        }

        public T GetOptions<T>()
        {
            return _configurationRoot.GetSection(typeof(T).Name).Get<T>();
        }
    }
}
