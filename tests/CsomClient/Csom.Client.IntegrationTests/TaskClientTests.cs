using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class GivenATaskClientInstance : IDisposable
    {
        private const string NamePrefix = "INT_TSK_TEST_";

        private readonly IProjectClient _projectClient;
        private readonly ITaskClient _client;
        private readonly ICustomFieldClient _customFieldClient;
        private readonly IEntityTypeClient _entityTypeClient;

        private readonly PublishedProject _publishedProject;

        public GivenATaskClientInstance()
        {
            IApplicationContext applicationContext = new ApplicationContext();
            IConfigurationRoot configurationRoot = ConfigurationRootFactory.Create(applicationContext);
            IConfigurationGetter configurationGetter = new ConfigurationGetter(configurationRoot);

            var projectContextOptions = configurationGetter.GetOptions<ProjectContextOptions>();
            var clientOptions = configurationGetter.GetOptions<CsomClientOptions>();

            var projectContext = ProjectContextFactory.Build(projectContextOptions);
            _projectClient = new ProjectClient(projectContext, clientOptions);
            _client = new TaskClient(projectContext, clientOptions);
            _customFieldClient = new CustomFieldClient(projectContext, clientOptions);
            _entityTypeClient = new EntityTypeClient(projectContext, clientOptions);

            _publishedProject = CreateTestProject().Result;
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetAllAsync_ThenPublishedTasksAreReturned()
        {
            // ACT
            var result = await _client.GetAllAsync(_publishedProject);

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetByIdAsync_IfExistPublishedTaskWithRightId_ThenIsReturned()
        {
            // ARRANGE
            var taskModel = new TaskModel()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString()
            };
            await _client.AddAsync(_publishedProject, taskModel);

            // ACT
            var result = await _client.GetByIdAsync(_publishedProject, taskModel.Id);

            // ASSERT
            Assert.Equal(taskModel.Id, result.Id);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetByIdAsync_IfNotExist_ThenNullIsReturned()
        {
            // ARRANGE
            var guid = Guid.NewGuid();

            // ACT
            var result = await _client.GetByIdAsync(_publishedProject, guid);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetByNameAsync_IfExistPublishedTaskWithRightName_ThenIsReturned()
        {
            // ARRANGE
            var taskModel = new TaskModel()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString()
            };
            await _client.AddAsync(_publishedProject, taskModel);

            // ACT
            var result = await _client.GetByNameAsync(_publishedProject, taskModel.Name);

            // ASSERT
            Assert.Equal(taskModel.Name, result.Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetByNameAsync_IfNotExist_ThenNullIsReturned()
        {
            // ARRANGE
            var name = Guid.NewGuid().ToString();

            // ACT
            var result = await _client.GetByNameAsync(_publishedProject, name);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsync_ThenPublishedTaskIsReturned()
        {
            // ARRANGE
            var taskModel = new TaskModel()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
                Start = DateTime.Today.AddDays(2).Date,
                Finish = DateTime.Today.AddDays(4).Date
            };

            // ACT
            var publishedTask = await _client.AddAsync(_publishedProject, taskModel);

            // ASSERT
            Assert.NotNull(publishedTask);
            Assert.Equal(taskModel.Id, publishedTask.Id);
            Assert.Equal(taskModel.Name, publishedTask.Name);
            Assert.Equal(taskModel.Start.Date, publishedTask.Start.Date);
            Assert.Equal(taskModel.Finish.Date, publishedTask.Finish.Date);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsyncWithCustomFields_ThenPublishedTaskIsReturnedWithCustomFields()
        {
            // ARRANGE
            var taskModel = new TaskModel()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString()
            };

            var customFieldTextValue = Guid.NewGuid().ToString();
            var customFields =  await _customFieldClient.GetAllByEntityTypeAsync((await _entityTypeClient.GetAllAsync()).TaskEntity);
            var customField = customFields.First(cf => cf.FieldType == CustomFieldType.TEXT);
            taskModel.CustomFields = new Dictionary<CustomField, object>() { { customField, customFieldTextValue } };
            
            // ACT
            var publishedTask = await _client.AddAsync(_publishedProject, taskModel);

            // ASSERT
            Assert.NotNull(publishedTask);
            Assert.Equal(publishedTask[customField.InternalName], customFieldTextValue);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsyncWithProperties_ThenPublishedTaskIsReturnedWithProperties()
        {
            // ARRANGE
            var taskModel = new TaskModel()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
                Priority = 666
                // TODO: Find out with tests which properties can be set on creation and have an add task model and update task model
            };
            
            // ACT
            var publishedTask = await _client.AddAsync(_publishedProject, taskModel);

            // ASSERT
            Assert.NotNull(publishedTask);
            Assert.Equal(taskModel.Priority, publishedTask.Priority);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsync_IfIdExists_ThenNullIsReturned()
        {
            // ARRANGE
            var taskModel = new TaskModel()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString()
            };
            var taskModel2 = new TaskModel()
            {
                Id = taskModel.Id,
                Name = NamePrefix + Guid.NewGuid().ToString()
            };

            // ACT
            await _client.AddAsync(_publishedProject, taskModel);
            var publishedTaskSecondTime = await _client.AddAsync(_publishedProject, taskModel2);

            // ASSERT
            Assert.Null(publishedTaskSecondTime);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddRangeAsync_ThenPublishedTasksAreReturned()
        {
            // ARRANGE
            var taskModel = new TaskModel()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString()
            };
            var taskModel2 = new TaskModel()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString()
            };

            // ACT
            var publishedTasks = await _client.AddRangeAsync(_publishedProject, new List<TaskModel>() { taskModel, taskModel2 });

            // ASSERT
            Assert.NotNull(publishedTasks);
            Assert.Equal(2, publishedTasks.Count());
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenRemoveAsync_ThenPublishedTaskIsRemoved()
        {
            // ARRANGE
            var taskModel = new TaskModel()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString()
            };

            // ACT
            var publishedTask = await _client.AddAsync(_publishedProject, taskModel);
            var result = await _client.RemoveAsync(_publishedProject, publishedTask);
            publishedTask = await _client.GetByIdAsync(_publishedProject, taskModel.Id);

            // ASSERT
            Assert.True(result);
            Assert.Null(publishedTask);
        }

        private Task<PublishedProject> CreateTestProject()
        {
            var newProject = new ProjectModel()
            {
                Id = Guid.NewGuid(),
                Name = NamePrefix + Guid.NewGuid().ToString(),
                Start = DateTime.Today.AddDays(1).Date
            };
            return _projectClient.AddAsync(newProject);
        }

        public void Dispose()
        {
            // Clean all published projects creating for the text execution
            // NOTE: THIS WILL WORK ONLY IF GETALL AND REMOVE METHOD WORKS
            var publishedProject = _projectClient.GetAllAsync().Result;

            foreach (var createdProject in publishedProject.Where(pp => pp.Name.StartsWith(NamePrefix)))
            {
                _projectClient.RemoveAsync(createdProject).Wait();
            }
        }
    }
}
