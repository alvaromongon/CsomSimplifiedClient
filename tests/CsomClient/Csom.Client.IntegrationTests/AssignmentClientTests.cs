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
    public class GivenAnAssignmentClientInstance : IDisposable
    {
        private const string PrjNamePrefix = "INT_PRJ_ASS_TEST_";
        private const string ResNamePrefix = "INT_RES_ASS_TEST_";

        private readonly IProjectClient _projectClient;
        private readonly ITaskClient _taskClient;
        private readonly IEnterpriseResourceClient _resourceClient;
        private readonly IAssignmentClient _client;

        private readonly PublishedProject _publishedProject;
        private readonly PublishedTask _publishedTask;
        private readonly EnterpriseResource _enterpriseResource;
        private readonly EnterpriseResource _enterpriseResource2;

        public GivenAnAssignmentClientInstance()
        {
            IApplicationContext applicationContext = new ApplicationContext();
            IConfigurationRoot configurationRoot = ConfigurationRootFactory.Create(applicationContext);
            IConfigurationGetter configurationGetter = new ConfigurationGetter(configurationRoot);

            var projectContextOptions = configurationGetter.GetOptions<ProjectContextOptions>();
            var clientOptions = configurationGetter.GetOptions<CsomClientOptions>();

            var projectContext = ProjectContextFactory.Build(projectContextOptions);

            _projectClient = new ProjectClient(projectContext, clientOptions);
            _taskClient = new TaskClient(projectContext, clientOptions);
            _resourceClient = new EnterpriseResourceClient(projectContext, clientOptions);

            _client = new AssignmentClient(projectContext, clientOptions);

            _publishedProject = CreateTestProject().Result;
            _publishedTask = CreateTestTask().Result;

            _enterpriseResource = CreateTestResource().Result;
            _enterpriseResource2 = CreateTestResource().Result;

            _publishedProject = _projectClient.LinkToEnterpriseResources(_publishedProject, new []{ _enterpriseResource, _enterpriseResource2 }).Result;
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetAllAsync_ThenPublishedAssignmentsAreReturned()
        {
            // ACT
            var result = await _client.GetAllAsync(_publishedTask);

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenGetByIdAsync_IfExistPublishedAssignmentWithRightId_ThenIsReturned()
        {
            // ARRANGE
            var assignmentModel = new AssignmentModel()
            {
                Id = Guid.NewGuid(),
                ResourceId = _enterpriseResource.Id,
                TaskId = _publishedTask.Id,
                Start = DateTime.Now,
                Finish = DateTime.Now.AddDays(1)
            };
            await _client.AddAsync(_publishedProject, assignmentModel);

            // ACT
            var result = await _client.GetByIdAsync(_publishedProject, assignmentModel.Id);

            // ASSERT
            Assert.Equal(assignmentModel.Id, result.Id);
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
        public async System.Threading.Tasks.Task WhenAddAsync_ThenPublishedAssignmentIsReturned()
        {
            // ARRANGE
            var assignmentModel = new AssignmentModel()
            {
                Id = Guid.NewGuid(),
                ResourceId = _enterpriseResource.Id,
                TaskId = _publishedTask.Id,
                Start = DateTime.Now,
                Finish = DateTime.Now.AddDays(1)                
            };

            // ACT
            var publishedAssignment = await _client.AddAsync(_publishedProject, assignmentModel);

            // ASSERT
            Assert.NotNull(publishedAssignment);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsyncWithProperties_ThenPublishedAssignmentIsReturnedWithProperties()
        {
            // ARRANGE
            var assignmentModel = new AssignmentModel()
            {
                Id = Guid.NewGuid(),
                ResourceId = _enterpriseResource.Id,
                TaskId = _publishedTask.Id,
                Start = DateTime.Today.AddDays(2).Date,
                Finish = DateTime.Today.AddDays(3).Date,
                ResourceCapacity = 666
            };

            // ACT
            var publishedAssignment = await _client.AddAsync(_publishedProject, assignmentModel);

            // ASSERT
            Assert.NotNull(publishedAssignment);
            Assert.Equal(assignmentModel.ResourceCapacity, publishedAssignment.ResourceCapacity);
            Assert.Equal(assignmentModel.Start.Date, publishedAssignment.Start.Date);
            Assert.Equal(assignmentModel.Finish.Date, publishedAssignment.Finish.Date);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddAsync_IfIdExists_ThenNullIsReturned()
        {
            // ARRANGE
            var assignmentModel = new AssignmentModel()
            {
                Id = Guid.NewGuid(),
                ResourceId = _enterpriseResource.Id,
                TaskId = _publishedTask.Id,
                Start = DateTime.Now,
                Finish = DateTime.Now.AddDays(1)
            };
            var assignmentModel2 = new AssignmentModel()
            {
                Id = assignmentModel.Id,
                ResourceId = _enterpriseResource.Id,
                TaskId = _publishedTask.Id,
                Start = DateTime.Now,
                Finish = DateTime.Now.AddDays(1)
            };

            // ACT
            await _client.AddAsync(_publishedProject, assignmentModel);
            var publishedAssignemntSecondTime = await _client.AddAsync(_publishedProject, assignmentModel2);

            // ASSERT
            Assert.Null(publishedAssignemntSecondTime);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddRangeSameResourceAsyncToSameTask_ThenPublishedAssignmentsAreReturned()
        {
            // ARRANGE
            var assignmentModel = new AssignmentModel()
            {
                Id = Guid.NewGuid(),
                ResourceId = _enterpriseResource.Id,
                TaskId = _publishedTask.Id,
                Start = DateTime.Now,
                Finish = DateTime.Now.AddDays(1)
            };

            var assignmentModel2 = new AssignmentModel()
            {
                Id = Guid.NewGuid(),
                ResourceId = _enterpriseResource.Id,
                TaskId = _publishedTask.Id,
                Start = DateTime.Now,
                Finish = DateTime.Now.AddDays(1)
            };

            // ACT
            var publishedAssignments = await _client.AddRangeAsync(_publishedProject, 
                new List<AssignmentModel>() { assignmentModel, assignmentModel2 });

            // ASSERT
            Assert.NotNull(publishedAssignments);
            Assert.Single(publishedAssignments);
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddRangeDifferentResourcesAsyncToSameTask_ThenPublishedAssignmentsAreReturned()
        {
            // ARRANGE
            var assignmentModel = new AssignmentModel()
            {
                Id = Guid.NewGuid(),
                ResourceId = _enterpriseResource.Id,
                TaskId = _publishedTask.Id,
                Start = DateTime.Now.Date,
                Finish = DateTime.Now.AddDays(1).Date
            };            
            
            var assignmentModel2 = new AssignmentModel()
            {
                Id = Guid.NewGuid(),
                ResourceId = _enterpriseResource2.Id, //DIFFERENT RESOURCE
                TaskId = _publishedTask.Id,
                Start = DateTime.Now.AddDays(2).Date,
                Finish = DateTime.Now.AddDays(3).Date
            };

            // ACT
            var publishedAssignments = await _client.AddRangeAsync(_publishedProject,
                new List<AssignmentModel>() { assignmentModel, assignmentModel2 });

            // ASSERT
            Assert.NotNull(publishedAssignments);
            Assert.Equal(2, publishedAssignments.Count());
        }

        [Fact]
        public async System.Threading.Tasks.Task WhenAddRangeDifferentResourcesAsyncToDifferentTasks_ThenPublishedAssignmentsAreReturned()
        {
            // ARRANGE
            var assignmentModel = new AssignmentModel()
            {
                Id = Guid.NewGuid(),
                ResourceId = _enterpriseResource.Id,
                TaskId = _publishedTask.Id,
                Start = DateTime.Now.Date,
                Finish = DateTime.Now.AddDays(1).Date
            };

            var publishedTask2 = await CreateTestTask();
            var assignmentModel2 = new AssignmentModel()
            {
                Id = Guid.NewGuid(),
                ResourceId = _enterpriseResource2.Id, //DIFFERENT RESOURCE
                TaskId = publishedTask2.Id, // DIFFERENT TASK
                Start = DateTime.Now.AddDays(2).Date,
                Finish = DateTime.Now.AddDays(3).Date
            };            

            // ACT
            var publishedAssignments = await _client.AddRangeAsync(_publishedProject,
                new List<AssignmentModel>() { assignmentModel, assignmentModel2 });

            // ASSERT
            Assert.NotNull(publishedAssignments);
            Assert.Equal(2, publishedAssignments.Count());
        }


        [Fact]
        public async System.Threading.Tasks.Task WhenRemoveAsync_ThenPublishedAssignmentIsRemoved()
        {
            // ARRANGE
            var assignmentModel = new AssignmentModel()
            {
                Id = Guid.NewGuid(),
                ResourceId = _enterpriseResource.Id,
                TaskId = _publishedTask.Id,
                Start = DateTime.Now,
                Finish = DateTime.Now.AddDays(1)
            };

            // ACT
            var publishedAssignment = await _client.AddAsync(_publishedProject, assignmentModel);
            var result = await _client.RemoveAsync(_publishedProject, publishedAssignment);
            publishedAssignment = await _client.GetByIdAsync(_publishedProject, assignmentModel.Id);

            // ASSERT
            Assert.True(result);
            Assert.Null(publishedAssignment);
        }

        public void Dispose()
        {
            // NOTE: THIS WILL WORK ONLY IF GETALL AND REMOVE METHOD WORKS

            // Clean all published projects created for the text execution
            var publishedProject = _projectClient.GetAllAsync().Result;

            foreach (var createdProject in publishedProject.Where(pp => pp.Name.StartsWith(PrjNamePrefix)))
            {
                _projectClient.RemoveAsync(createdProject).Wait();
            }

            // Clean all enterprise resources created for the text execution
            var enterpriseResources = _resourceClient.GetAllAsync().Result;

            foreach (var enterpriseResource in enterpriseResources.Where(pp => pp.Name.StartsWith(ResNamePrefix)))
            {
                _resourceClient.RemoveAsync(enterpriseResource).Wait();
            }
        }        

        private Task<PublishedProject> CreateTestProject()
        {
            var projectModel = new ProjectModel()
            {
                Id = Guid.NewGuid(),
                Name = PrjNamePrefix + Guid.NewGuid().ToString(),
                Start = DateTime.Today.Date
            };
            return _projectClient.AddAsync(projectModel);
        }

        private Task<PublishedTask> CreateTestTask()
        {
            var taskModel = new TaskModel()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
                Start = DateTime.Today.AddDays(1).Date,
                Finish = DateTime.Today.AddDays(4).Date,
                IsManual = false,
                Priority = 500,
                Work = "0h",
                ConstraintType = ConstraintType.AsLateAsPossible
            };

            return _taskClient.AddAsync(_publishedProject, taskModel);
        }

        private Task<EnterpriseResource> CreateTestResource()
        {
            var enterpriseResourceModel = new EnterpriseResourceModel()
            {
                Id = Guid.NewGuid(),
                IsBudget = false,
                IsGeneric = false,
                IsInactive = false,
                Name = ResNamePrefix + Guid.NewGuid().ToString(),
                ResourceType = EnterpriseResourceType.Material,
                Initials = "R",
                Group = "ER group",
                DefaultBookingType = BookingType.Committed,
                CostRate = new CostRateCreationInformation()
                {
                    StandardRate = 1,
                    CostPerUse = 0 // These costs are typically one-time costs that are not based on rates.
                }
            };

            return _resourceClient.AddAsync(enterpriseResourceModel);
        }
    }
}
