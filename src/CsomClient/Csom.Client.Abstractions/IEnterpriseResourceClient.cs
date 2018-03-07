using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ProjectServer.Client;
using Csom.Client.Model;

namespace Csom.Client.Abstractions
{
    public interface IEnterpriseResourceClient
    {
        Task<IEnumerable<EnterpriseResource>> GetAllAsync();

        Task<EnterpriseResource> GetByIdAsync(Guid id);

        Task<IEnumerable<EnterpriseResource>> GetAllByTypeAsync(EnterpriseResourceType resourceType);

        Task<EnterpriseResource> GetByNameAsync(string name);

        Task<EnterpriseResource> AddAsync(EnterpriseResourceModel enterpriseResourceModel);

        Task<IEnumerable<EnterpriseResource>> AddRangeAsync(IEnumerable<EnterpriseResourceModel> enterpriseResourceModels);

        Task<bool> RemoveAsync(EnterpriseResource enterpriseResource);
    }
}
