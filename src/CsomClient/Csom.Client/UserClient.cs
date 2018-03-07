using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;
using Csom.Client.Abstractions;
using Csom.Client.Common;

namespace Csom.Client
{
    public class UserClient : BaseClient, IUserClient
    {
        public UserClient(ProjectContext projectContext, CsomClientOptions options) : base(projectContext, options)
        {
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var users = _projectContext.Web.SiteUsers;
            _projectContext.Load(users);
            await _projectContext.ExecuteQueryAsync();
            
            return users;
        }
    }
}
