using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ProjectServer.Client;
using Csom.Client.Model;

namespace Csom.Client.Abstractions
{
    public interface IAssignmentClient
    {
        Task<IEnumerable<PublishedAssignment>> GetAllAsync(PublishedTask publishedTask);

        Task<IEnumerable<PublishedAssignment>> GetAllAsync(PublishedProject publishedProject);

        Task<PublishedAssignment> GetByIdAsync(PublishedProject publishedProject, Guid id);

        Task<PublishedAssignment> AddAsync(PublishedProject publishedProject, AssignmentModel assignmentModel);

        Task<IEnumerable<PublishedAssignment>> AddRangeAsync(PublishedProject publishedProject, IEnumerable<AssignmentModel> assignmentModels);

        Task<bool> RemoveAsync(PublishedProject publishedProject, PublishedAssignment publishedAssignment);
    }
}
