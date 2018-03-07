using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ProjectServer.Client;
using Csom.Client.Model;

namespace Csom.Client.Abstractions
{
    public interface ITaskClient
    {
        Task<IEnumerable<PublishedTask>> GetAllAsync(PublishedProject publishedProject, bool includeCustomFields = false);

        Task<PublishedTask> GetByIdAsync(PublishedProject publishedProject, Guid id, bool includeCustomFields = false);

        Task<PublishedTask> GetByNameAsync(PublishedProject publishedProject, string name, bool includeCustomFields = false);

        Task<PublishedTask> AddAsync(PublishedProject publishedProject, TaskModel task);

        Task<IEnumerable<PublishedTask>> AddRangeAsync(PublishedProject publishedProject, IEnumerable<TaskModel> taskModels);

        Task<bool> RemoveAsync(PublishedProject publishedProject, PublishedTask publishedTask);
    }
}
