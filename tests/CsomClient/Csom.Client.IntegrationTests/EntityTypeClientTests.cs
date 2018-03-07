using Microsoft.Extensions.Configuration;
using Common.AppContext;
using Common.Configuration;
using Csom.Client.Abstractions;
using Csom.Client.Common;
using Csom.Client.Context;
using Xunit;

namespace Csom.Client.IntegrationTests
{
    public class GivenAnEntityTypeClientInstance
    {
        private readonly IEntityTypeClient _client;

        public GivenAnEntityTypeClientInstance()
        {
            IApplicationContext applicationContext = new ApplicationContext();
            IConfigurationRoot configurationRoot = ConfigurationRootFactory.Create(applicationContext);
            IConfigurationGetter configurationGetter = new ConfigurationGetter(configurationRoot);

            var projectContextOptions = configurationGetter.GetOptions<ProjectContextOptions>();
            var clientOptions = configurationGetter.GetOptions<CsomClientOptions>();

            var projectContext = ProjectContextFactory.Build(projectContextOptions);
            _client = new EntityTypeClient(projectContext, clientOptions);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetAllAsync_ThenEntityTypesAreReturned()
        {
            // ACT
            var result = await _client.GetAllAsync();

            // ASSERT
            Assert.NotNull(result);
        }
    }
}
