using Microsoft.Extensions.Configuration;
using Common.AppContext;
using Common.Configuration;
using Csom.Client.Abstractions;
using Csom.Client.Common;
using Csom.Client.Context;
using Xunit;

namespace Csom.Client.IntegrationTests
{
    public class GivenAnUserClientInstance
    {
        private readonly IUserClient _client;

        public GivenAnUserClientInstance()
        {
            IApplicationContext applicationContext = new ApplicationContext();
            IConfigurationRoot configurationRoot = ConfigurationRootFactory.Create(applicationContext);
            IConfigurationGetter configurationGetter = new ConfigurationGetter(configurationRoot);

            var projectContextOptions = configurationGetter.GetOptions<ProjectContextOptions>();
            var clientOptions = configurationGetter.GetOptions<CsomClientOptions>();

            var projectContext = ProjectContextFactory.Build(projectContextOptions);
            _client = new UserClient(projectContext, clientOptions);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetAllAsync_ThenUsersAreReturned()
        {
            // ACT
            var result = await _client.GetAllAsync();

            // ASSERT
            Assert.NotNull(result);
        }
    }
}
