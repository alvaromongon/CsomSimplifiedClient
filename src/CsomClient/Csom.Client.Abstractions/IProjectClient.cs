using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ProjectServer.Client;
using Csom.Client.Model;

namespace Csom.Client.Abstractions
{
    public interface IProjectClient
    {
        Task<IEnumerable<PublishedProject>> GetAllAsync();

        Task<PublishedProject> GetByIdAsync(Guid id);

        Task<PublishedProject> GetByNameAsync(string name);

        //Task<bool> CheckInAsync(PublishedProject publishedProject);

        //Task<bool> CheckOutAsync(PublishedProject publishedProject);

        Task<PublishedProject> LinkToEnterpriseResource(PublishedProject publishedProject, EnterpriseResource enterpriseResource);

        Task<PublishedProject> LinkToEnterpriseResources(PublishedProject publishedProject, IEnumerable<EnterpriseResource> enterpriseResources);

        Task<PublishedProject> AddAsync(ProjectModel projectModel);        

        Task<bool> RemoveAsync(PublishedProject publishedProject);        
    }
}
