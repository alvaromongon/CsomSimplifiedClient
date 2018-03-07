using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.ProjectServer.Client;
using Common.AppContext;
using Common.Configuration;
using Csom.Client.Abstractions;
using Csom.Client.Common;
using Csom.Client.Context;
using Csom.Client.Extensions;
using Csom.Client.Model;
using Xunit;

namespace Csom.Client.IntegrationTests
{
    public class GivenAnEnterpriseResourceClientInstance : IDisposable
    {
        private const string NamePrefix = "INT_ERS_TEST_";

        private readonly IEnterpriseResourceClient _client;
        private readonly ICustomFieldClient _customFieldClient;
        private readonly IEntityTypeClient _entityTypeClient;

        public GivenAnEnterpriseResourceClientInstance()
        {
            IApplicationContext applicationContext = new ApplicationContext();
            IConfigurationRoot configurationRoot = ConfigurationRootFactory.Create(applicationContext);
            IConfigurationGetter configurationGetter = new ConfigurationGetter(configurationRoot);

            var projectContextOptions = configurationGetter.GetOptions<ProjectContextOptions>();
            var clientOptions = configurationGetter.GetOptions<CsomClientOptions>();

            var projectContext = ProjectContextFactory.Build(projectContextOptions);
            _client = new EnterpriseResourceClient(projectContext, clientOptions);
            _customFieldClient = new CustomFieldClient(projectContext, clientOptions);
            _entityTypeClient = new EntityTypeClient(projectContext, clientOptions);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetAllAsync_ThenEnterpriseResourcesAreReturned()
        {
            // ACT
            var result = await _client.GetAllAsync();

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetByIdAsync_IfExistEnterpriseResourceWithRightId_ThenIsReturned()
        {
            // ARRANGE
            var model = new EnterpriseResourceModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                ResourceType = EnterpriseResourceType.Work
            };
            await _client.AddAsync(model);

            // ACT
            var result = await _client.GetByIdAsync(model.Id);

            // ASSERT
            Assert.Equal(model.Id, result.Id);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetByIdAsync_IfNotExist_ThenNullIsReturned()
        {
            // ARRANGE
            var guid = Guid.NewGuid();

            // ACT
            var result = await _client.GetByIdAsync(guid);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetByNameAsync_IfExistEnterpriseResourceWithRightName_ThenIsReturned()
        {
            // ARRANGE
            var model = new EnterpriseResourceModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                ResourceType = EnterpriseResourceType.Material
            };
            await _client.AddAsync(model);

            // ACT
            var result = await _client.GetByNameAsync(model.Name);

            // ASSERT
            Assert.Equal(model.Name, result.Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetByNameAsync_IfNotExist_ThenNullIsReturned()
        {
            // ARRANGE
            var name = Guid.NewGuid().ToString();

            // ACT
            var result = await _client.GetByNameAsync(name);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetByTypeAsync_IfExistEnterpriseResourcesWithRightType_ThenAreReturned()
        {
            // ARRANGE
            var model = new EnterpriseResourceModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                ResourceType = EnterpriseResourceType.Cost
            };
            await _client.AddAsync(model);

            // ACT
            var result = await _client.GetAllByTypeAsync(EnterpriseResourceType.Cost);

            // ASSERT
            Assert.True(result.All(cf => cf.ResourceType == EnterpriseResourceType.Cost));
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsync_ThenEnterpriseResourceIsReturned()
        {
            // ARRANGE
            var model = new EnterpriseResourceModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                ResourceType = EnterpriseResourceType.Cost
            };

            // ACT
            var enterpriseResource = await _client.AddAsync(model);

            // ASSERT
            Assert.NotNull(enterpriseResource);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsyncWithProperties_ThenEnterpriseResourceIsReturnedWithProperties()
        {
            // ARRANGE
            var model = new EnterpriseResourceModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                ResourceType = EnterpriseResourceType.Material,
                Initials = "PMP"
            };

            // ACT
            var enterpriseResource = await _client.AddAsync(model);

            // ASSERT
            Assert.NotNull(enterpriseResource);
            Assert.Equal(model.Initials, enterpriseResource.Initials);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsyncWithCostRate_ThenEnterpriseResourceIsReturnedWithCostRate()
        {
            // ARRANGE
            var model = new EnterpriseResourceModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                ResourceType = EnterpriseResourceType.Material,
                CostRate = new CostRateCreationInformation()
                {
                    StandardRate = 1,
                    EffectiveDate = DateTime.Now
                }
            };

            // ACT
            var enterpriseResource = await _client.AddAsync(model);

            // ASSERT
            Assert.NotNull(enterpriseResource);
            Assert.Equal(
                model.CostRate.StandardRate, 
                (await enterpriseResource.GetDefaultCostRateTableAsync()).CostRates.First().StandardRate);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsync_IfIdExists_ThenNullIsReturned()
        {
            // ARRANGE
            var model = new EnterpriseResourceModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                ResourceType = EnterpriseResourceType.Material
            };
            var model2 = new EnterpriseResourceModel()
            {
                Id = model.Id,
                Name = NamePrefix + Guid.NewGuid().ToString(),
                ResourceType = EnterpriseResourceType.Material
            };

            // ACT
            await _client.AddAsync(model);
            var enterpriseResourceSecondTime = await _client.AddAsync(model2);

            // ASSERT
            Assert.Null(enterpriseResourceSecondTime);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddRangeAsync_ThenPublishedTasksAreReturned()
        {
            // ARRANGE
            var model = new EnterpriseResourceModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                ResourceType = EnterpriseResourceType.Material
            };
            var model2 = new EnterpriseResourceModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                ResourceType = EnterpriseResourceType.Material
            };

            // ACT
            var enterpriseResources = await _client.AddRangeAsync(new List<EnterpriseResourceModel>() { model, model2 });

            // ASSERT
            Assert.NotNull(enterpriseResources);
            Assert.Equal(2, enterpriseResources.Count());
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenRemoveAsync_ThenPublishedTaskIsRemoved()
        {
            // ARRANGE
            var model = new EnterpriseResourceModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                ResourceType = EnterpriseResourceType.Material
            };

            // ACT
            var enterpriseResource = await _client.AddAsync(model);
            var result = await _client.RemoveAsync(enterpriseResource);

            // ASSERT
            Assert.True(result);
        }

        public void Dispose()
        {
            // Clean all custom fields created for this test execution 
            // NOTE: THIS WILL WORK ONLY IF GETALL AND REMOVE METHOD WORKS
            var resources = _client.GetAllAsync().Result;

            foreach (var createdCustomField in resources.Where(cf => cf.Name.StartsWith(NamePrefix)))
            {
                _client.RemoveAsync(createdCustomField).Wait();
            }
        }
    }
}
