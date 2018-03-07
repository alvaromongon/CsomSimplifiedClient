using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ProjectServer.Client;

namespace Csom.Client.Abstractions
{
    public interface ICustomFieldClient
    {
        Task<IEnumerable<CustomField>> GetAllAsync();

        Task<IEnumerable<CustomField>> GetAllByEntityTypeAsync(EntityType entityType);

        Task<CustomField> GetByIdAsync(Guid id);

        Task<CustomField> GetByNameAsync(string name);

        Task<CustomField> AddAsync(CustomFieldCreationInformation creationInformation);

        Task<bool> RemoveAsync(CustomField customField);
    }
}
