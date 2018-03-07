using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace Csom.Client.Abstractions
{
    public interface IUserClient
    {
        Task<IEnumerable<User>> GetAllAsync();
    }
}
