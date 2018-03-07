using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.ProjectServer.Client;
using Common.AppContext;
using Common.Configuration;
using Csom.Client.Abstractions;
using Csom.Client.Common;
using Csom.Client.Context;
using Xunit;

namespace Csom.Client.IntegrationTests
{
    public class GivenACustomFieldClientInstance : IDisposable
    {
        private const string NamePrefix = "INT_CFD_TEST_";

        private readonly ICustomFieldClient _client;
        private readonly IEntityTypeClient _entityTypeClient;

        public GivenACustomFieldClientInstance()
        {
            IApplicationContext applicationContext = new ApplicationContext();
            IConfigurationRoot configurationRoot = ConfigurationRootFactory.Create(applicationContext);
            IConfigurationGetter configurationGetter = new ConfigurationGetter(configurationRoot);

            var projectContextOptions = configurationGetter.GetOptions<ProjectContextOptions>();
            var clientOptions = configurationGetter.GetOptions<CsomClientOptions>();

            var projectContext = ProjectContextFactory.Build(projectContextOptions);
            _client = new CustomFieldClient(projectContext, clientOptions);
            _entityTypeClient = new EntityTypeClient(projectContext, clientOptions);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetAllAsync_ThenCustomFieldsAreReturned()
        {
            // ACT
            var result = await _client.GetAllAsync();

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetByIdAsync_IfExistCustomFieldWithRightId_ThenIsReturned()
        {
            // ARRANGE
            var creationInformation = new CustomFieldCreationInformation()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                FieldType = CustomFieldType.TEXT
            };
            await _client.AddAsync(creationInformation);

            // ACT
            var result = await _client.GetByIdAsync(creationInformation.Id);

            // ASSERT
            Assert.Equal(creationInformation.Id, result.Id);
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
        public async System.Threading.Tasks.Task WhenGetByNameAsync_IfExistCustomFieldWithRightName_ThenIsReturned()
        {
            // ARRANGE
            var creationInformation = new CustomFieldCreationInformation()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                FieldType = CustomFieldType.TEXT
            };
            await _client.AddAsync(creationInformation);

            // ACT
            var result = await _client.GetByNameAsync(creationInformation.Name);

            // ASSERT
            Assert.Equal(creationInformation.Name, result.Name);
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
        public async System.Threading.Tasks.Task WhenGetByEntityTypeAsync_IfExistCustomFieldsWithRightType_ThenAreReturned()
        {
            // ARRANGE
            var entityType = (await _entityTypeClient.GetAllAsync()).TaskEntity;
            var creationInformation = new CustomFieldCreationInformation()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                FieldType = CustomFieldType.TEXT,
                EntityType = entityType
            };
            await _client.AddAsync(creationInformation);            

            // ACT
            var result = await _client.GetAllByEntityTypeAsync(entityType);

            // ASSERT
            Assert.True(result.All(cf => cf.EntityType.ID == entityType.ID));
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsync_ThenCustomFieldIsReturned()
        {
            // ARRANGE
            var customFieldCreationInformation = new CustomFieldCreationInformation()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                FieldType = CustomFieldType.TEXT
            };

            // ACT
            var publishedProject = await _client.AddAsync(customFieldCreationInformation);

            // ASSERT
            Assert.NotNull(publishedProject);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsync_IfNotEntityTypeIsDefined_ThenArgumentNullExceptionIsThrown()
        {
            // ARRANGE
            var customFieldCreationInformation = new CustomFieldCreationInformation()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString()
            };

            // ACT & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _client.AddAsync(customFieldCreationInformation));
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsync_IfIdExists_ThenNullIsReturned()
        {
            // ARRANGE
            var customFieldCreationInformation = new CustomFieldCreationInformation()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                FieldType = CustomFieldType.TEXT
            };
            var customFieldCreationInformation2 = new CustomFieldCreationInformation()
            {
                Id = customFieldCreationInformation.Id,
                Name = NamePrefix + Guid.NewGuid().ToString(),
                FieldType = CustomFieldType.TEXT
            };

            // ACT
            await _client.AddAsync(customFieldCreationInformation);
            var customFieldSecondTime = await _client.AddAsync(customFieldCreationInformation2);

            // ASSERT
            Assert.Null(customFieldSecondTime);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsync_IfNameExists_ThenNullIsReturned()
        {
            // ARRANGE
            var customFieldCreationInformation = new CustomFieldCreationInformation()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                FieldType = CustomFieldType.TEXT
            };
            var customFieldCreationInformation2 = new CustomFieldCreationInformation()
            {
                Id = Guid.NewGuid(),
                Name = customFieldCreationInformation.Name,
                FieldType = CustomFieldType.TEXT
            };

            // ACT
            await _client.AddAsync(customFieldCreationInformation);
            var customFieldSecondTime = await _client.AddAsync(customFieldCreationInformation2);

            // ASSERT
            Assert.Null(customFieldSecondTime);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenRemoveAsync_ThenCustomFieldIsRemoved()
        {
            // ARRANGE
            var customFieldCreationInformation = new CustomFieldCreationInformation()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                FieldType = CustomFieldType.TEXT
            };

            // ACT
            var customField = await _client.AddAsync(customFieldCreationInformation);
            var result = await _client.RemoveAsync(customField);
            customField = await _client.GetByIdAsync(customFieldCreationInformation.Id);

            // ASSERT
            Assert.True(result);
            Assert.Null(customField);
        }

        public void Dispose()
        {
            // Clean all custom fields created for this test execution 
            // NOTE: THIS WILL WORK ONLY IF GETALL AND REMOVE METHOD WORKS
            var customFields = _client.GetAllAsync().Result;

            foreach (var createdCustomField in customFields.Where(cf=> cf.Name.StartsWith(NamePrefix)))
            {
                _client.RemoveAsync(createdCustomField).Wait();
            }
        }
    }
}
