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
    public class ProjectClient : BaseClient, IProjectClient
    {
        private readonly IMapper _mapper;

        public ProjectClient(ProjectContext projectContext, CsomClientOptions options) : base(projectContext, options)
        {
            _mapper = MapperFactory.BuildTaskMapper();
        }

        public async Task<IEnumerable<PublishedProject>> GetAllAsync()
        {
            IEnumerable<PublishedProject> publishedProjects = new List<PublishedProject>();

            try
            {
                _projectContext.Load(_projectContext.Projects);
                await _projectContext.ExecuteQueryAsync();

                // Force a new list to avoid returning same list than stored in context
                publishedProjects = new List<PublishedProject>(_projectContext.Projects.ToArray());
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting projects. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return publishedProjects;
        }

        public async Task<PublishedProject> GetByIdAsync(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            PublishedProject publishedProject = null;

            try
            {
                IEnumerable<PublishedProject> publishedProjects =
                    _projectContext.LoadQuery(_projectContext.Projects.Where(p => p.Id == id));
                await _projectContext.ExecuteQueryAsync();

                if (publishedProjects.Any())
                {
                    publishedProject = publishedProjects.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting a project by id. " +
                    $"id searched is {id}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return publishedProject;
        }

        public async Task<PublishedProject> GetByNameAsync(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            PublishedProject publishedProject = null;

            try
            {
                IEnumerable<PublishedProject> publishedProjects =
                    _projectContext.LoadQuery(_projectContext.Projects.Where(p => p.Name == name));
                await _projectContext.ExecuteQueryAsync();

                if (publishedProjects.Any())
                {
                    publishedProject = publishedProjects.FirstOrDefault();
                }

            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting a project by name. " +
                    $"Name searched is {name}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return publishedProject;
        }

        public async Task<bool> CheckInAsync(PublishedProject publishedProject)
        {
            if (publishedProject == null)
            {
                throw new ArgumentNullException(nameof(publishedProject));
            }

            bool result = false;

            try
            {
                if (publishedProject.IsCheckedOut)
                {
                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        var queuedJob = publishedProject.Draft.CheckIn(true);
                        WaitForQueuedJobToComplete(queuedJob, $"adding a checking in a project with name {publishedProject.Name}");
                    });
                }                

                result = true;
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error removing a project. " +
                    $"project id is {publishedProject.Id}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return result;
        }

        public async Task<DraftProject> CheckOutAsync(PublishedProject publishedProject)
        {
            if (publishedProject == null)
            {
                throw new ArgumentNullException(nameof(publishedProject));
            }

            DraftProject draftProject = null;

            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    draftProject = publishedProject.CheckOut();
                });
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error checking out a project. " +
                    $"project id is {publishedProject.Id}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return draftProject;
        }

        internal System.Threading.Tasks.Task PublishAndUpdate(DraftProject draftProject, string projectName)
        {
            if (draftProject == null)
            {
                throw new ArgumentNullException(nameof(draftProject));
            }

            try
            {
                return System.Threading.Tasks.Task.Run(() => {
                    // https://nearbaseline.com/2014/08/ciconotcheckedout-queue-errors-when-updating-projects-via-jsom/
                    // TODO: Do I need to do update and publish?
                    var queuedJob = draftProject.Publish(false);
                    WaitForQueuedJobToComplete(queuedJob, $"publishing a change in a project with name {projectName}");

                    //queuedJob = _projectContext.Projects.Update();
                    //WaitForQueuedJobToComplete(queuedJob, $"updating a project with name {projectName}");
                });                
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error publishing and updating draft project. " +
                    $"draft project id is {draftProject.Id}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }
        }

        public Task<PublishedProject> LinkToEnterpriseResource(PublishedProject publishedProject, EnterpriseResource enterpriseResource)
        {            
            return LinkToEnterpriseResources(publishedProject, new List<EnterpriseResource>() { enterpriseResource });
        }

        //http://projectservercode.com/add-enterprise-resource-to-project-team-using-csom/
        public async Task<PublishedProject> LinkToEnterpriseResources(PublishedProject publishedProject, IEnumerable<EnterpriseResource> enterpriseResources)
        {
            if (publishedProject == null)
            {
                throw new ArgumentNullException(nameof(publishedProject));
            }

            if (enterpriseResources == null)
            {
                throw new ArgumentNullException(nameof(enterpriseResources));
            }

            PublishedProject result = null;

            try
            {
                await CheckInAsync(publishedProject);
                var draftProject = await CheckOutAsync(publishedProject);

                foreach(var er in enterpriseResources)
                {
                    draftProject.ProjectResources.AddEnterpriseResource(er);
                }
                draftProject.Update();

                _projectContext.Load(draftProject);
                await _projectContext.ExecuteQueryAsync();

                await PublishAndUpdate(draftProject, publishedProject.Name);
                await CheckInAsync(publishedProject);

                result = await GetByIdAsync(publishedProject.Id);
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error linking an enterprise resource to project. " +
                    $"project id is {publishedProject.Id}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return result;
        }

        // http://projectservercode.com/create-project-in-project-online-using-csom/
        // https://msdn.microsoft.com/en-us/library/office/jj163064.aspx
        public async Task<PublishedProject> AddAsync(ProjectModel projectModel)
        {
            if (projectModel == null)
            {
                throw new ArgumentNullException(nameof(projectModel));
            }

            PublishedProject publishedProject = null;

            try
            {
                // Ensure no other project with same id or name exist
                if ((!await AlreadyExist(projectModel.Id, projectModel.Name)))
                {
                    publishedProject = _projectContext.Projects.Add(_mapper.Map<ProjectCreationInformation>(projectModel));

                    var queuedJob = _projectContext.Projects.Update();
                    WaitForQueuedJobToComplete(queuedJob, $"adding a project with name {projectModel.Name}");
                    
                    // We currently do not have extra properties
                    //var draftProject = await CheckOutAsync(publishedProject);
                    //draftProject = _mapper.Map(projectModel, draftProject);
                    //await PublishAndUpdate(draftProject, projectModel.Name);
                    
                    // Need to retrieve published project information before check it in
                    publishedProject = await GetByIdAsync(projectModel.Id);
                }
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error adding a new project. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return publishedProject;
        }

        private async Task<bool> AlreadyExist(Guid id, string name)
        {
            _projectContext.Load(_projectContext.Projects);
            var projects = _projectContext.LoadQuery(_projectContext.Projects.Where(pr =>
                pr.Id == id || pr.Name == name));
            await _projectContext.ExecuteQueryAsync();

            return projects.Any();
        }

        public async Task<bool> RemoveAsync(PublishedProject publishedProject)
        {
            if (publishedProject == null)
            {
                throw new ArgumentNullException(nameof(publishedProject));
            }

            bool result = false;

            try
            {
                await CheckInAsync(publishedProject);
                var clientResult = _projectContext.Projects.Remove(publishedProject);
                var queuedJob = _projectContext.Projects.Update();
                WaitForQueuedJobToComplete(queuedJob, $"adding a removing a project with name {publishedProject.Name}");
                await _projectContext.ExecuteQueryAsync();
                result = clientResult.Value;
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error removing a project. " +
                    $"project id is {publishedProject.Id}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return result;
        }
    }
}
