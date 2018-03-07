using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.ProjectServer.Client;
using Common.AppContext;
using Common.Configuration;
using Csom.Client.Abstractions;
using Csom.Client.Common;
using Csom.Client.Context;
using Csom.Client.Model;
using Xunit;

namespace Csom.Client.IntegrationTests
{
    public class GivenAProjectClientInstance : IDisposable
    {
        private const string NamePrefix = "INT_PRJ_TEST_";
        private const string EnterpriseResourceNamePrefix = "INT_PRJ_ER_TEST_";

        private readonly ProjectContext _projectContext;
        private readonly IProjectClient _client;
        private readonly IEnterpriseResourceClient _enterpriseResourceClient;

        public GivenAProjectClientInstance()
        {
            IApplicationContext applicationContext = new ApplicationContext();
            IConfigurationRoot configurationRoot = ConfigurationRootFactory.Create(applicationContext);
            IConfigurationGetter configurationGetter = new ConfigurationGetter(configurationRoot);

            var projectContextOptions = configurationGetter.GetOptions<ProjectContextOptions>();
            var clientOptions = configurationGetter.GetOptions<CsomClientOptions>();

            _projectContext = ProjectContextFactory.Build(projectContextOptions);
            _client = new ProjectClient(_projectContext, clientOptions);
            _enterpriseResourceClient = new EnterpriseResourceClient(_projectContext, clientOptions);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetAllAsync_ThenPublishedProjectsAreReturned()
        {
            // ACT
            var result = await _client.GetAllAsync();

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetByIdAsync_IfExistPublishedProjectWithRightId_ThenIsReturned()
        {
            // ARRANGE
            var projectModel = new ProjectModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString()
            };
            await _client.AddAsync(projectModel);

            // ACT
            var result = await _client.GetByIdAsync(projectModel.Id);

            // ASSERT
            Assert.Equal(projectModel.Id, result.Id);
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
        public async System.Threading.Tasks.Task WhenGetByNameAsync_IfExistPublishedProjectsWithRightName_ThenIsReturned()
        {
            // ARRANGE
            var projectModel = new ProjectModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString()
            };
            await _client.AddAsync(projectModel);

            // ACT
            var result = await _client.GetByNameAsync(projectModel.Name);

            // ASSERT
            Assert.Equal(projectModel.Name, result.Name);
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
        public async System.Threading.Tasks.Task WhenLinkToEnterpriseResourceAsync_ThenTrueIsReturned()
        {
            // ARRANGE
            var projectModel = new ProjectModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString()
            };
            var enterpriseResource = (await _enterpriseResourceClient.GetAllAsync()).First();

            // ACT
            var publishedProject = await _client.AddAsync(projectModel);
            var result = await _client.LinkToEnterpriseResource(publishedProject, enterpriseResource);

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenLinkToEnterpriseResourceAsyncTwice_ThenTrueIsReturned()
        {
            // ARRANGE
            var projectModel = new ProjectModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString()
            };
            var enterpriseResource = (await _enterpriseResourceClient.GetAllAsync()).First();

            // ACT
            var publishedProject = await _client.AddAsync(projectModel);
            var result = await _client.LinkToEnterpriseResource(publishedProject, enterpriseResource);
            var result2 = await _client.LinkToEnterpriseResource(publishedProject, enterpriseResource);

            // ASSERT
            Assert.NotNull(result);
            Assert.NotNull(result2);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenLinkToEnterpriseResourceWithPropertiesAsync_ThenPropertiesAreAppliedOnProjectResource()
        {
            // ARRANGE
            var model = new EnterpriseResourceModel()
            {
                Id = Guid.NewGuid(),
                IsBudget = false,
                IsGeneric = false,
                IsInactive = false,
                Name = EnterpriseResourceNamePrefix + Guid.NewGuid().ToString(),
                ResourceType = EnterpriseResourceType.Material,
                Initials = "I",
                Group = "AGroup",
                DefaultBookingType = BookingType.Committed,
                CostRate = new CostRateCreationInformation()
                {
                    StandardRate = 1,
                    CostPerUse = 0
                }
            };
            var enterpriseResource = await _enterpriseResourceClient.AddAsync(model);

            var projectModel = new ProjectModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString()
            };
            var publishedProject = await _client.AddAsync(projectModel);

            // ACT
            var result = await _client.LinkToEnterpriseResource(publishedProject, enterpriseResource);
            publishedProject = await _client.GetByIdAsync(publishedProject.Id);
            _projectContext.Load(publishedProject.ProjectResources);
            await _projectContext.ExecuteQueryAsync();
            var projectResource = publishedProject.ProjectResources.FirstOrDefault();

            // ASSERT
            Assert.NotNull(result);
            Assert.NotNull(projectResource);
            Assert.Equal(model.Id, projectResource.Id);
            Assert.Equal(model.IsBudget, projectResource.IsBudgeted);
            Assert.Equal(model.IsGeneric, projectResource.IsGenericResource);
            Assert.Equal(model.Initials, projectResource.Initials);
            Assert.Equal(model.Group, projectResource.Group);
            Assert.Equal(model.DefaultBookingType, projectResource.DefaultBookingType);
            Assert.Equal(model.CostRate.StandardRate, projectResource.StandardRate);
            Assert.Equal(model.CostRate.CostPerUse, projectResource.CostPerUse);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsync_ThenPublishedProjectIsReturned()
        {
            // ARRANGE
            var projectModel = new ProjectModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                Description = "Description",
                Start = DateTime.Today.Date
            };

            // ACT
            var publishedProject = await _client.AddAsync(projectModel);

            // ASSERT
            Assert.NotNull(publishedProject);
            Assert.Equal(projectModel.Id, publishedProject.Id);
            Assert.Equal(projectModel.Name, publishedProject.Name);
            Assert.Equal(projectModel.Description, publishedProject.Description);
            Assert.Equal(projectModel.Start.Date, publishedProject.DefaultStartTime.Date);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsync_IfIdExists_ThenNullIsReturned()
        {
            // ARRANGE
            var projectModel = new ProjectModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString()
            };
            var projectModel2 = new ProjectModel()
            {
                Id = projectModel.Id,
                Name = NamePrefix + Guid.NewGuid().ToString()
            };

            // ACT
            await _client.AddAsync(projectModel);
            var publishedProjectSecondTime = await _client.AddAsync(projectModel2);

            // ASSERT
            Assert.Null(publishedProjectSecondTime);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsync_IfNameExists_ThenNullIsReturned()
        {
            // ARRANGE
            var projectModel = new ProjectModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString()
            };
            var projectModel2 = new ProjectModel()
            {
                Id = Guid.NewGuid(),
                Name = projectModel.Name
            };

            // ACT
            await _client.AddAsync(projectModel);
            var publishedProjectSecondTime = await _client.AddAsync(projectModel2);

            // ASSERT
            Assert.Null(publishedProjectSecondTime);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenRemoveAsync_ThenPublishedProjectIsRemoved()
        {
            // ARRANGE
            var projectModel = new ProjectModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString()
            };

            // ACT
            var publishedProject = await _client.AddAsync(projectModel);
            var result = await _client.RemoveAsync(publishedProject);
            publishedProject = await _client.GetByIdAsync(projectModel.Id);

            // ASSERT
            Assert.True(result);
            Assert.Null(publishedProject);
        }

        public void Dispose()
        {
            // Clean all published projects creating for the text execution
            // NOTE: THIS WILL WORK ONLY IF GETALL AND REMOVE METHOD WORKS
            var publishedProject = _client.GetAllAsync().Result;

            foreach (var createdProject in publishedProject.Where(pp => pp.Name.StartsWith(NamePrefix)))
            {
                _client.RemoveAsync(createdProject).Wait();
            }

            var enterpriseResources = _enterpriseResourceClient.GetAllAsync().Result;

            foreach (var enterpriseResource in enterpriseResources.Where(pp => pp.Name.StartsWith(EnterpriseResourceNamePrefix)))
            {
                _enterpriseResourceClient.RemoveAsync(enterpriseResource).Wait();
            }
        }
    }
}
