using System;
using System.Threading.Tasks;
using Microsoft.ProjectServer.Client;
using Csom.Client.Abstractions;
using Csom.Client.Common;

namespace Csom.Client
{
    public class EntityTypeClient : BaseClient, IEntityTypeClient
    {        
        public EntityTypeClient(ProjectContext projectContext, CsomClientOptions options) : base(projectContext, options)
        {}

        public async Task<EntityTypes> GetAllAsync()
        {
            EntityTypes entityTypes = null;

            try
            {
                _projectContext.Load(_projectContext.EntityTypes);
                await _projectContext.ExecuteQueryAsync();

                _projectContext.Load(_projectContext.EntityTypes.ProjectEntity);
                _projectContext.Load(_projectContext.EntityTypes.TaskEntity);
                _projectContext.Load(_projectContext.EntityTypes.ResourceEntity);
                await _projectContext.ExecuteQueryAsync();

                entityTypes = _projectContext.EntityTypes;
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting entity types. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return entityTypes;
        }
    }
}
