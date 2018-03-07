using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;
using Csom.Client.Abstractions;
using Csom.Client.Common;
using Csom.Client.Mapper;
using Csom.Client.Model;

namespace Csom.Client
{
    public class AssignmentClient : BaseClient, IAssignmentClient
    {
        private readonly ProjectClient _projectClient;
        private readonly TaskClient _taskClient;
        private readonly EnterpriseResourceClient _resourceClient;

        private readonly IMapper _mapper;

        public AssignmentClient(ProjectContext projectContext, CsomClientOptions options) : base(projectContext, options)
        {
            // TODO: This is breaking the not new instances rule, should I expose it?
            _projectClient = new ProjectClient(projectContext, options);
            _taskClient = new TaskClient(projectContext, options);
            _resourceClient = new EnterpriseResourceClient(projectContext, options);

            _mapper = MapperFactory.BuildTaskMapper();
        }

        public async Task<IEnumerable<PublishedAssignment>> GetAllAsync(PublishedProject publishedProject)
        {
            if (publishedProject == null)
            {
                throw new ArgumentNullException(nameof(publishedProject));
            }

            IEnumerable<PublishedAssignment> assignments = new List<PublishedAssignment>();

            try
            {
                _projectContext.Load(publishedProject.Assignments);
                await _projectContext.ExecuteQueryAsync();

                // Force a new list to avoid returning same list than stored in context
                assignments = new List<PublishedAssignment>(publishedProject.Assignments.ToArray());
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting all project assignments. " +
                    $"Published project name is {publishedProject.Name}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return assignments;
        }

        public async Task<IEnumerable<PublishedAssignment>> GetAllAsync(PublishedTask publishedTask)
        {
            if (publishedTask == null)
            {
                throw new ArgumentNullException(nameof(publishedTask));
            }

            IEnumerable<PublishedAssignment> assignments = new List<PublishedAssignment>();

            try
            {
                _projectContext.Load(publishedTask.Assignments);
                await _projectContext.ExecuteQueryAsync();

                // Force a new list to avoid returning same list than stored in context
                assignments = new List<PublishedAssignment>(publishedTask.Assignments.ToArray());
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting all task assignments. " +
                    $"Published task name is {publishedTask.Name}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return assignments;
        }

        public async Task<PublishedAssignment> GetByIdAsync(PublishedProject publishedProject, Guid id)
        {
            if (publishedProject == null)
            {
                throw new ArgumentNullException(nameof(publishedProject));
            }
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            PublishedAssignment assignment = null;

            try
            {
                IEnumerable<PublishedAssignment> publishedAssignments = _projectContext.LoadQuery(
                    publishedProject.Assignments.Where(a => a.Id == id));
                await _projectContext.ExecuteQueryAsync();

                if (publishedAssignments.Any())
                {
                    assignment = publishedAssignments.FirstOrDefault();

                    // We need this in order to be able to locate the task the assigment belongs to
                    _projectContext.Load(assignment.Task);
                    await _projectContext.ExecuteQueryAsync();
                }
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting an assignment by id. " +
                    $"id searched is {id}. " +
                    $"Published task name is {publishedProject.Name}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return assignment;
        }

        // http://projectservercode.com/assign-resource-on-task-using-csom/
        public async Task<PublishedAssignment> AddAsync(PublishedProject publishedProject, AssignmentModel assignmentModel)
        {
            var result = await AddRangeAsync(publishedProject, new List<AssignmentModel>() { assignmentModel });

            return result?.FirstOrDefault();
        }        

        public async Task<IEnumerable<PublishedAssignment>> AddRangeAsync(PublishedProject publishedProject, IEnumerable<AssignmentModel> assignmentModels)
        {
            if (publishedProject == null)
            {
                throw new ArgumentNullException(nameof(publishedProject));
            }
            if (assignmentModels == null)
            {
                throw new ArgumentNullException(nameof(assignmentModels));
            }

            List<PublishedAssignment> result = new List<PublishedAssignment>();

            try
            {                
                if ((await RequirementsAreMet(publishedProject, assignmentModels)))
                {
                    // Ensure I have it check it out
                    await _projectClient.CheckInAsync(publishedProject);
                    var draftProject = await _projectClient.CheckOutAsync(publishedProject);

                    foreach (var assignmentModel in assignmentModels)
                    {                        
                        var draftAssignment = draftProject.Assignments.Add(_mapper.Map<AssignmentCreationInformation>(assignmentModel));
                        draftAssignment = _mapper.Map(assignmentModel, draftAssignment);

                        //var queuedJob = draftProject.Update();
                        //WaitForQueuedJobToComplete(queuedJob, $"updating a project with name {publishedProject.Name}");
                        //_projectContext.ExecuteQuery();
                    }

                    await _projectClient.PublishAndUpdate(draftProject, publishedProject.Name);
                    await _projectClient.CheckInAsync(publishedProject);

                    // reload the project after re-publishing it
                    var _republishedProjects = _projectContext.LoadQuery(
                        _projectContext.Projects
                        .Where(p => p.Id == publishedProject.Id).IncludeWithDefaultProperties(p => p.Assignments));
                    await _projectContext.ExecuteQueryAsync();

                    result = _republishedProjects.First().Assignments
                        .Where(a => assignmentModels.Any(am => am.Id == a.Id)).ToList();
                }                
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error adding a new assignments. " +
                    $"Published project name is {publishedProject.Name}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return result;
        }        

        public async Task<bool> RemoveAsync(PublishedProject publishedProject, PublishedAssignment publishedAssignment)
        {
            if (publishedProject == null)
            {
                throw new ArgumentNullException(nameof(publishedProject));
            }
            if (publishedAssignment == null)
            {
                throw new ArgumentNullException(nameof(publishedAssignment));
            }

            bool result = false;

            try
            {
                _projectContext.Load(publishedProject.Assignments);
                await _projectContext.ExecuteQueryAsync();

                if (publishedProject.Assignments.Any(ass => ass.Id == publishedAssignment.Id))
                {
                    // Ensure I have it check it out
                    await _projectClient.CheckInAsync(publishedProject);
                    var draftProject = await _projectClient.CheckOutAsync(publishedProject);

                    _projectContext.Load(draftProject.Assignments);
                    var draftAssignments = _projectContext
                        .LoadQuery(draftProject.Assignments.Where(ass => ass.Id == publishedAssignment.Id));
                    await _projectContext.ExecuteQueryAsync();

                    var draftAssignment = draftAssignments.First();
                    var clientResult = draftProject.Assignments.Remove(draftAssignment);

                    await _projectClient.PublishAndUpdate(draftProject, publishedProject.Name);
                    await _projectClient.CheckInAsync(publishedProject);

                    result = clientResult.Value;
                }          
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error removing an assignment. " +
                    $"Assignment id is {publishedAssignment.Id}. " +
                    $"Published project name is {publishedProject.Name}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return result;
        }

        private async Task<bool> RequirementsAreMet(PublishedProject publishedProject, IEnumerable<AssignmentModel> assignmentModels)
        {
            bool result = false;

            // Ensure task id belongs to project
            if (publishedProject != null)
            {
                // Ensure resource ids exist
                var resourceTasks = assignmentModels.Select(async (am) => await _resourceClient.GetByIdAsync(am.ResourceId));
                var resoures = await System.Threading.Tasks.Task.WhenAll(resourceTasks);
                
                if (resoures.All(r => r != null))
                {
                    // Ensure no other assignments with same ids exist
                    _projectContext.Load(publishedProject.Assignments);
                    await _projectContext.ExecuteQueryAsync();
                    var draftAssignment = publishedProject.Assignments.Where(dt => assignmentModels.Any(am => am.Id == dt.Id));
                    if (draftAssignment == null || !draftAssignment.Any())
                    {
                        result = true;
                    }
                }                
            }

            return result;
        }        
    }
}
