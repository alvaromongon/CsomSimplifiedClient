using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ProjectServer.Client;
using Csom.Client.Abstractions;
using Csom.Client.Common;
using Csom.Client.Mapper;
using Csom.Client.Model;

namespace Csom.Client
{
    public class TaskClient : BaseClient, ITaskClient
    {
        private readonly ProjectClient _projectClient;
        private readonly IMapper _mapper;

        public TaskClient(ProjectContext projectContext, CsomClientOptions options) : base(projectContext, options)
        {
            // TODO: This is breaking the not new instances rule, should I expose it?
            _projectClient = new ProjectClient(projectContext, options);

            _mapper = MapperFactory.BuildTaskMapper();
        }

        public async Task<IEnumerable<PublishedTask>> GetAllAsync(PublishedProject publishedProject, bool includeCustomFields = false)
        {
            if (publishedProject == null)
            {
                throw new ArgumentNullException(nameof(publishedProject));
            }

            IEnumerable<PublishedTask> publishedTasks = new List<PublishedTask>();

            try
            {
                _projectContext.Load(publishedProject.Tasks);                
                await _projectContext.ExecuteQueryAsync();               

                // Force a new list to avoid returning same list than stored in context
                publishedTasks = new List<PublishedTask>(publishedProject.Tasks.ToArray());

                if (includeCustomFields)
                {
                    foreach (var publishedTask in publishedTasks)
                    {
                        await LoadCustomFields(publishedTask);
                    }
                }                    
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting tasks. " +
                    $"Published project name is {publishedProject.Name}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return publishedTasks;
        }

        public async Task<PublishedTask> GetByIdAsync(PublishedProject publishedProject, Guid id, bool includeCustomFields = false)
        {
            if (publishedProject == null)
            {
                throw new ArgumentNullException(nameof(publishedProject));
            }
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            PublishedTask publishedTask = null;

            try
            {
                IEnumerable<PublishedTask> publishedTasks = _projectContext.LoadQuery(
                    publishedProject.Tasks.Where(t => t.Id == id));
                await _projectContext.ExecuteQueryAsync();

                if (publishedTasks.Any())
                {
                    publishedTask = publishedTasks.FirstOrDefault();
                    if (includeCustomFields)
                    {
                        await LoadCustomFields(publishedTask);
                    }                    
                }
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting a task by id. " +
                    $"id searched is {id}. " +
                    $"Published project name is {publishedProject.Name}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return publishedTask;
        }        

        public async Task<PublishedTask> GetByNameAsync(PublishedProject publishedProject, string name, bool includeCustomFields = false)
        {
            if (publishedProject == null)
            {
                throw new ArgumentNullException(nameof(publishedProject));
            }
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            PublishedTask publishedTask = null;

            try
            {
                IEnumerable<PublishedTask> publishedTasks =
                    _projectContext.LoadQuery(publishedProject.Tasks.Where(p => p.Name == name));
                await _projectContext.ExecuteQueryAsync();

                if (publishedTasks.Any())
                {
                    publishedTask = publishedTasks.FirstOrDefault();
                    if (includeCustomFields)
                    {
                        await LoadCustomFields(publishedTask);
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting a task by name. " +
                    $"name searched is {name}. " +
                    $"Published project name is {publishedProject.Name}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return publishedTask;
        }

        //http://projectservercode.com/update-project-task-in-project-online-using-csom/
        public async Task<PublishedTask> AddAsync(PublishedProject publishedProject, TaskModel task)
        {
            var result = await AddRangeAsync(publishedProject, new List<TaskModel>() { task });

            return result?.FirstOrDefault();
        }        

        public async Task<IEnumerable<PublishedTask>> AddRangeAsync(PublishedProject publishedProject, IEnumerable<TaskModel> taskModels)
        {
            if (publishedProject == null)
            {
                throw new ArgumentNullException(nameof(publishedProject));
            }
            if (taskModels == null)
            {
                throw new ArgumentNullException(nameof(taskModels));
            }

            IList<PublishedTask> publishedTasks = new List<PublishedTask>();

            IList<Guid> publishedTaskIds = new List<Guid>();
            try
            {
                // Ensure I have it check it out
                await _projectClient.CheckInAsync(publishedProject);
                var draftProject = await _projectClient.CheckOutAsync(publishedProject);

                _projectContext.Load(draftProject.Tasks);
                await _projectContext.ExecuteQueryAsync();

                var existingProjectTaskIds = draftProject.Tasks.Select(t => t.Id).ToList();
                foreach (var taskModel in taskModels)
                {
                    // Ensure no other task with same id exist
                    if (!existingProjectTaskIds.Any(id => id == taskModel.Id))
                    {
                        var draftTask = draftProject.Tasks.Add(_mapper.Map<TaskCreationInformation>(taskModel));
                        draftTask = _mapper.Map(taskModel, draftTask);
                        SetCustomFields(draftTask, taskModel.CustomFields);

                        publishedTaskIds.Add(taskModel.Id);
                    }                                        
                }

                if (publishedTaskIds.Any())
                {
                    await _projectClient.PublishAndUpdate(draftProject, publishedProject.Name);

                    foreach (var id in publishedTaskIds)
                    {
                        publishedTasks.Add(await GetByIdAsync(publishedProject, id, includeCustomFields: true));
                    }
                }
                await _projectClient.CheckInAsync(publishedProject);
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error adding a tasks. " +
                    $"Published project name is {publishedProject.Name}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return publishedTasks;
        }

        public async Task<bool> RemoveAsync(PublishedProject publishedProject, PublishedTask publishedTask)
        {
            if (publishedProject == null)
            {
                throw new ArgumentNullException(nameof(publishedProject));
            }
            if (publishedTask == null)
            {
                throw new ArgumentNullException(nameof(publishedTask));
            }

            bool result = false;

            try
            {
                // Ensure the task with the same id exist
                var task = _projectContext.LoadQuery(publishedProject.Tasks.
                    Where(pr => pr.Id == publishedTask.Id));
                await _projectContext.ExecuteQueryAsync();

                if (task.Any())
                {
                    // Ensure I have it check it out
                    await _projectClient.CheckInAsync(publishedProject);
                    var draftProject = await _projectClient.CheckOutAsync(publishedProject);

                    _projectContext.Load(draftProject.Tasks);
                    var draftTask = _projectContext.LoadQuery(draftProject.Tasks.Where(pr => pr.Id == publishedTask.Id));
                    await _projectContext.ExecuteQueryAsync();

                    var clientResult = draftProject.Tasks.Remove(draftTask.First());

                    await _projectClient.PublishAndUpdate(draftProject, publishedProject.Name);
                    await _projectClient.CheckInAsync(publishedProject);

                    result = clientResult.Value;
                }                   
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error removing a task. " +
                    $"project id is {publishedProject.Id}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return result;
        }

        private async System.Threading.Tasks.Task LoadCustomFields(PublishedTask publishedTask)
        {
            _projectContext.Load(publishedTask.CustomFields);
            await _projectContext.ExecuteQueryAsync();
        }

        private DraftTask SetCustomFields(DraftTask draftTask, IDictionary<CustomField, object> customFields)
        {
            if (customFields != null)
            {
                foreach (var customField in customFields)
                {
                    draftTask[customField.Key.InternalName] = customField.Value;
                }
            }

            return draftTask;
        }
    }
}
